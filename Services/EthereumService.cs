using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public class EthereumService : IEthereumService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EthereumService> _logger;
    private Web3? _web3;

    public EthereumService(
        IConfiguration configuration,
        ILogger<EthereumService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Web3> GetWeb3ClientAsync()
    {
        if (_web3 != null)
            return _web3;

        var rpcUrl = _configuration["BTCPayServer:Ethereum:RpcUrl"] 
            ?? throw new InvalidOperationException("Ethereum RPC URL not configured");

        _web3 = new Web3(rpcUrl);
        _logger.LogInformation("Ethereum Web3 client initialized with RPC: {RpcUrl}", rpcUrl);
        
        return _web3;
    }

    public async Task<string> GetCurrentBlockNumberAsync()
    {
        var web3 = await GetWeb3ClientAsync();
        var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        return blockNumber.Value.ToString();
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        var web3 = await GetWeb3ClientAsync();
        var balance = await web3.Eth.GetBalance.SendRequestAsync(address);
        return Web3.Convert.FromWei(balance.Value);
    }

    public async Task<string> GenerateNewAddressAsync()
    {
        // Generate new Ethereum account
        var account = new Account(Nethereum.Signer.EthECKey.GenerateKey());
        _logger.LogInformation("Generated new Ethereum address: {Address}", account.Address);
        return account.Address;
    }
}
