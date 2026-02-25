using BTCPayServer.Payments;
using BTCPayServer.Plugins.EthereumPayments.Models;
using BTCPayServer.Services.Invoices;
using NBitcoin;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Plugins.EthereumPayments.PaymentMethods;

public class EthereumPaymentMethodHandler : IPaymentMethodHandler
{
    public PaymentMethodId PaymentMethodId => EthereumPaymentType.ETH;

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

        var details = new EthereumPaymentMethodDetails
        {
            DepositAddress = settings.ReceivingAddress,
            PaymentType = "ETH"
        };

        return Task.FromResult<IPaymentMethodDetails>(details);
    }

    public object PreparePayment(PaymentMethodData supportedPaymentMethod, Data.StoreData store, PaymentMethodData[] invoicePaymentMethods)
    {
        return new { };
    }

    public void PreparePaymentModel(PaymentModel model, InvoiceResponse invoiceResponse, Data.StoreBlob storeBlob, PaymentMethodData paymentMethod)
    {
        var details = (EthereumPaymentMethodDetails)paymentMethod.GetPaymentMethodDetails();
        model.PaymentMethodName = "Ethereum";
        model.CryptoImage = "/img/ethereum.svg";
        model.InvoiceBitcoinUrl = $"ethereum:{details.DepositAddress}?value={model.CryptoCode}";
        model.InvoiceBitcoinUrlQR = model.InvoiceBitcoinUrl;
    }

    public string GetCryptoImage(PaymentMethodId paymentMethodId) => "/img/ethereum.svg";

    public string GetPaymentMethodName(PaymentMethodId paymentMethodId) => "Ethereum";

    public IEnumerable<PaymentMethodId> GetSupportedPaymentMethods() => new[] { EthereumPaymentType.ETH };

    public CheckoutUIPaymentMethodSettings GetCheckoutUISettings() => new CheckoutUIPaymentMethodSettings
    {
        ExtensionPartial = "Ethereum/EthereumLikeMethodCheckout",
        CheckoutBodyVueComponentName = "EthereumLikeMethodCheckout",
        CheckoutHeaderVueComponentName = "EthereumLikeMethodCheckoutHeader",
        NoScriptPartialName = "Ethereum/EthereumLikeMethodCheckoutNoScript"
    };

    private EthereumSettings? GetSettings(Data.StoreData store)
    {
        var blob = store.GetStoreBlob();
        var settings = blob.GetAdditionalData<EthereumSettings>("Ethereum");
        return settings;
    }
}

public class EthereumPaymentMethodDetails : IPaymentMethodDetails
{
    public string DepositAddress { get; set; } = string.Empty;
    public string PaymentType { get; set; } = "ETH";
    
    public string GetPaymentDestination() => DepositAddress;
    public PaymentType GetPaymentType() => BTCPayServer.Payments.PaymentType.BTCLike;
    public decimal GetNextNetworkFee() => 0.001m; // Estimated gas fee
    public decimal GetFeeRate() => 0;
    public bool Activated { get; set; } = true;
}
