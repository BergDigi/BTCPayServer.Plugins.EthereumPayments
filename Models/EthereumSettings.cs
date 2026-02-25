namespace BTCPayServer.Plugins.EthereumPayments.Models;

public class EthereumSettings
{
    /// <summary>
    /// External Ethereum RPC endpoint URL
    /// </summary>
    public string RpcUrl { get; set; } = "https://public-eth.nownodes.io";

    /// <summary>
    /// Store's Ethereum receiving address (EOA)
    /// </summary>
    public string? ReceivingAddress { get; set; }

    /// <summary>
    /// USDT ERC-20 contract address on Ethereum Mainnet
    /// </summary>
    public string UsdtContractAddress { get; set; } = "0xdAC17F958D2ee523a2206206994597C13D831ec7";

    /// <summary>
    /// dEURO ERC-20 contract address on Ethereum Mainnet
    /// </summary>
    public string DEuroContractAddress { get; set; } = "0x643B1C5F2eA59A6868e1C8e8B51Bf37d52E76A5F";

    /// <summary>
    /// Block confirmation count for considering payment final
    /// </summary>
    public int ConfirmationCount { get; set; } = 12;

    /// <summary>
    /// Scan interval in seconds
    /// </summary>
    public int ScanIntervalSeconds { get; set; } = 15;
}
