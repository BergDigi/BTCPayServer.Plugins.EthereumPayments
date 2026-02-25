namespace BTCPayServer.Plugins.EthereumPayments.Models;

public class Erc20TokenConfig
{
    public string Symbol { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;
    public int Decimals { get; set; }
    public string PaymentMethodId { get; set; } = string.Empty;
}
