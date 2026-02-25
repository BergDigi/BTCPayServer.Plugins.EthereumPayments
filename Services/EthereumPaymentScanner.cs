using BTCPayServer.Data;
using BTCPayServer.Plugins.EthereumPayments.Models;
using BTCPayServer.Services.Invoices;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public class EthereumPaymentScanner
{
    private readonly ILogger<EthereumPaymentScanner> _logger;
    private readonly EthereumRpcService _rpcService;
    private readonly InvoiceRepository _invoiceRepository;
    private readonly ApplicationDbContextFactory _dbContextFactory;

    public EthereumPaymentScanner(
        ILogger<EthereumPaymentScanner> logger,
        EthereumRpcService rpcService,
        InvoiceRepository invoiceRepository,
        ApplicationDbContextFactory dbContextFactory)
    {
        _logger = logger;
        _rpcService = rpcService;
        _invoiceRepository = invoiceRepository;
        _dbContextFactory = dbContextFactory;
    }

    public async Task ScanForPaymentsAsync(string storeId, EthereumSettings settings)
    {
        if (string.IsNullOrEmpty(settings.ReceivingAddress))
        {
            _logger.LogWarning($"No receiving address configured for store {storeId}");
            return;
        }

        try
        {
            // Get pending invoices for this store
            await using var context = _dbContextFactory.CreateContext();
            
            var pendingInvoices = await context.Invoices
                .Where(i => i.StoreDataId == storeId && 
                           i.Status == InvoiceStatusLegacy.New)
                .ToListAsync();

            if (!pendingInvoices.Any())
            {
                _logger.LogDebug($"No pending invoices for store {storeId}");
                return;
            }

            _logger.LogInformation($"Scanning for payments on {pendingInvoices.Count} invoices");

            // Get current block number
            var latestBlock = await _rpcService.GetLatestBlockNumberAsync(settings.RpcUrl);
            var fromBlock = latestBlock > 1000 ? latestBlock - 1000 : 0; // Scan last ~1000 blocks

            // Scan for ETH transfers
            var ethTransfers = await _rpcService.ScanEthTransfersAsync(
                settings.RpcUrl,
                settings.ReceivingAddress,
                fromBlock,
                latestBlock
            );

            // Scan for USDT transfers
            var usdtTransfers = await _rpcService.ScanErc20TransfersAsync(
                settings.RpcUrl,
                settings.UsdtContractAddress,
                settings.ReceivingAddress,
                6, // USDT has 6 decimals
                fromBlock,
                latestBlock
            );

            // Scan for dEURO transfers
            var dEuroTransfers = await _rpcService.ScanErc20TransfersAsync(
                settings.RpcUrl,
                settings.DEuroContractAddress,
                settings.ReceivingAddress,
                18, // dEURO has 18 decimals (assumed)
                fromBlock,
                latestBlock
            );

            // Match transfers to invoices
            await MatchTransfersToInvoices(pendingInvoices, ethTransfers, usdtTransfers, dEuroTransfers, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error scanning for payments on store {storeId}");
        }
    }

    private async Task MatchTransfersToInvoices(
        List<InvoiceData> invoices,
        List<EthTransfer> ethTransfers,
        List<Erc20Transfer> usdtTransfers,
        List<Erc20Transfer> dEuroTransfers,
        EthereumSettings settings)
    {
        foreach (var invoice in invoices)
        {
            try
            {
                var invoiceEntity = await _invoiceRepository.GetInvoice(invoice.Id);
                if (invoiceEntity == null) continue;

                // Check each payment method
                foreach (var paymentMethod in invoiceEntity.GetPaymentMethods())
                {
                    var paymentMethodId = paymentMethod.GetId();
                    var due = paymentMethod.Calculate().Due;

                    if (due <= 0) continue; // Already paid

                    // Match ETH payments
                    if (paymentMethodId == PaymentMethods.EthereumPaymentType.ETH)
                    {
                        await CheckEthPayments(invoiceEntity, ethTransfers, due, settings);
                    }
                    // Match USDT payments
                    else if (paymentMethodId == PaymentMethods.EthereumPaymentType.USDT)
                    {
                        await CheckErc20Payments(invoiceEntity, usdtTransfers, due, settings, "USDT");
                    }
                    // Match dEURO payments
                    else if (paymentMethodId == PaymentMethods.EthereumPaymentType.DEURO)
                    {
                        await CheckErc20Payments(invoiceEntity, dEuroTransfers, due, settings, "dEURO");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing invoice {invoice.Id}");
            }
        }
    }

    private async Task CheckEthPayments(
        InvoiceEntity invoice,
        List<EthTransfer> transfers,
        decimal due,
        EthereumSettings settings)
    {
        foreach (var transfer in transfers)
        {
            // Check if payment amount matches (with 1% tolerance)
            var tolerance = due * 0.01m;
            if (transfer.Value >= (due - tolerance))
            {
                var confirmations = await _rpcService.GetTransactionConfirmationsAsync(
                    settings.RpcUrl,
                    transfer.TransactionHash
                );

                if (confirmations >= settings.ConfirmationCount)
                {
                    _logger.LogInformation(
                        $"ETH payment detected for invoice {invoice.Id}: {transfer.Value} ETH in tx {transfer.TransactionHash}"
                    );

                    // Mark invoice as paid (simplified - actual implementation needs payment entity creation)
                    // await _invoiceRepository.AddPayment(invoice.Id, transfer);
                }
            }
        }
    }

    private async Task CheckErc20Payments(
        InvoiceEntity invoice,
        List<Erc20Transfer> transfers,
        decimal due,
        EthereumSettings settings,
        string tokenSymbol)
    {
        foreach (var transfer in transfers)
        {
            var tolerance = due * 0.01m;
            if (transfer.Value >= (due - tolerance))
            {
                var confirmations = await _rpcService.GetTransactionConfirmationsAsync(
                    settings.RpcUrl,
                    transfer.TransactionHash
                );

                if (confirmations >= settings.ConfirmationCount)
                {
                    _logger.LogInformation(
                        $"{tokenSymbol} payment detected for invoice {invoice.Id}: {transfer.Value} {tokenSymbol} in tx {transfer.TransactionHash}"
                    );

                    // Mark invoice as paid
                    // await _invoiceRepository.AddPayment(invoice.Id, transfer);
                }
            }
        }
    }
}
