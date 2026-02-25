namespace BTCPayServer.Plugins.EthereumPayments.Models;

public class EthereumPaymentMethodData
{
    public string ReceivingAddress { get; set; } = string.Empty;
    public string? TokenContractAddress { get; set; }
    public string PaymentType { get; set; } = "ETH";
    public int TokenDecimals { get; set; } = 18;
}
