using System.Threading.Tasks;
using Nethereum.Web3;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public interface IEthereumService
{
    Task<Web3> GetWeb3ClientAsync();
    Task<string> GetCurrentBlockNumberAsync();
    Task<decimal> GetBalanceAsync(string address);
    Task<string> GenerateNewAddressAsync();
}
