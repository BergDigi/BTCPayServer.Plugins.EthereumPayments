using BTCPayServer.Data;
using BTCPayServer.Plugins.EthereumPayments.Models;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public class EthereumTransactionWatcher : BackgroundService
{
    private readonly ILogger<EthereumTransactionWatcher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _defaultScanInterval = TimeSpan.FromSeconds(15);

    public EthereumTransactionWatcher(
        ILogger<EthereumTransactionWatcher> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ethereum Transaction Watcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanAllStoresAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Ethereum Transaction Watcher");
            }

            await Task.Delay(_defaultScanInterval, stoppingToken);
        }

        _logger.LogInformation("Ethereum Transaction Watcher stopped");
    }

    private async Task ScanAllStoresAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<ApplicationDbContextFactory>();
        var scanner = scope.ServiceProvider.GetRequiredService<EthereumPaymentScanner>();

        await using var context = dbContextFactory.CreateContext();
        
        var stores = await context.Stores.ToListAsync();

        foreach (var store in stores)
        {
            try
            {
                var settings = store.GetStoreBlob().GetAdditionalData<EthereumSettings>("Ethereum");
                
                if (settings == null || string.IsNullOrEmpty(settings.ReceivingAddress))
                    continue;

                _logger.LogDebug($"Scanning store {store.Id} for Ethereum payments");
                await scanner.ScanForPaymentsAsync(store.Id, settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scanning store {store.Id}");
            }
        }
    }
}
