using BTCPayServer.Payments;
using BTCPayServer.Plugins.EthereumPayments.Models;
using BTCPayServer.Services.Invoices;
using NBitcoin;

namespace BTCPayServer.Plugins.EthereumPayments.PaymentMethods;

public class Erc20PaymentMethodHandler : IPaymentMethodHandler
{
    private readonly Erc20TokenConfig _tokenConfig;

    public Erc20PaymentMethodHandler()
    {
        // Will be configured via dependency injection for USDT and dEURO
        _tokenConfig = new Erc20TokenConfig();
    }

    public PaymentMethodId PaymentMethodId => new(_tokenConfig.PaymentMethodId);

    public Task<IPaymentMethodDetails> CreatePaymentMethodDetails(
        InvoiceLogs logs,
        PaymentMethodData paymentMethod,
        PaymentMethod[] supportedPaymentMethods,
        Data.StoreData store,
        Network network,
        object? preparePaymentObject,
        IEnumerable<PaymentMethodId> invoicePaymentMethods)
    {
        var settings = GetSettings(store);
        if (string.IsNullOrEmpty(settings?.ReceivingAddress))
        {
            throw new InvalidOperationException("Ethereum receiving address not configured");
        }

        var details = new Erc20PaymentMethodDetails
        {
            DepositAddress = settings.ReceivingAddress,
            TokenContractAddress = _tokenConfig.ContractAddress,
            TokenSymbol = _tokenConfig.Symbol,
            TokenDecimals = _tokenConfig.Decimals
        };

        return Task.FromResult<IPaymentMethodDetails>(details);
    }

    public object PreparePayment(PaymentMethodData supportedPaymentMethod, Data.StoreData store, PaymentMethodData[] invoicePaymentMethods)
    {
        return new { };
    }

    public void PreparePaymentModel(PaymentModel model, InvoiceResponse invoiceResponse, Data.StoreBlob storeBlob, PaymentMethodData paymentMethod)
    {
        var details = (Erc20PaymentMethodDetails)paymentMethod.GetPaymentMethodDetails();
        model.PaymentMethodName = $"{_tokenConfig.Symbol} (ERC-20)";
        model.CryptoImage = $"/img/{_tokenConfig.Symbol.ToLower()}.svg";
        model.InvoiceBitcoinUrl = $"ethereum:{details.DepositAddress}?token={details.TokenContractAddress}";
        model.InvoiceBitcoinUrlQR = model.InvoiceBitcoinUrl;
    }

    public string GetCryptoImage(PaymentMethodId paymentMethodId) => $"/img/{_tokenConfig.Symbol.ToLower()}.svg";

    public string GetPaymentMethodName(PaymentMethodId paymentMethodId) => $"{_tokenConfig.Symbol} (ERC-20)";

    public IEnumerable<PaymentMethodId> GetSupportedPaymentMethods() => new[] { PaymentMethodId };

    public CheckoutUIPaymentMethodSettings GetCheckoutUISettings() => new CheckoutUIPaymentMethodSettings
    {
        ExtensionPartial = "Ethereum/Erc20MethodCheckout",
        CheckoutBodyVueComponentName = "Erc20MethodCheckout",
        CheckoutHeaderVueComponentName = "Erc20MethodCheckoutHeader",
        NoScriptPartialName = "Ethereum/Erc20MethodCheckoutNoScript"
    };

    private EthereumSettings? GetSettings(Data.StoreData store)
    {
        var blob = store.GetStoreBlob();
        return blob.GetAdditionalData<EthereumSettings>("Ethereum");
    }
}

public class Erc20PaymentMethodDetails : IPaymentMethodDetails
{
    public string DepositAddress { get; set; } = string.Empty;
    public string TokenContractAddress { get; set; } = string.Empty;
    public string TokenSymbol { get; set; } = string.Empty;
    public int TokenDecimals { get; set; }
    
    public string GetPaymentDestination() => DepositAddress;
    public PaymentType GetPaymentType() => BTCPayServer.Payments.PaymentType.BTCLike;
    public decimal GetNextNetworkFee() => 0.001m;
    public decimal GetFeeRate() => 0;
    public bool Activated { get; set; } = true;
}
