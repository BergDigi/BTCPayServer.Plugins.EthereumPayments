using System.Threading.Tasks;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public interface IEthereumWalletService
{
    Task<string> CreateWalletAsync(string storeId);
    Task<string> GetDepositAddressAsync(string storeId);
    Task<decimal> GetWalletBalanceAsync(string storeId);
}
