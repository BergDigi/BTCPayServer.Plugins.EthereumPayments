using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public class EthereumWalletService : IEthereumWalletService
{
    private readonly IEthereumService _ethereumService;
    private readonly ILogger<EthereumWalletService> _logger;
    
    // In-Memory Wallet Storage (für MVP - später durch DB ersetzen)
    private readonly ConcurrentDictionary<string, string> _storeWallets = new();

    public EthereumWalletService(
        IEthereumService ethereumService,
        ILogger<EthereumWalletService> logger)
    {
        _ethereumService = ethereumService;
        _logger = logger;
    }

    public async Task<string> CreateWalletAsync(string storeId)
    {
        if (_storeWallets.ContainsKey(storeId))
        {
            _logger.LogWarning("Wallet for store {StoreId} already exists", storeId);
            return _storeWallets[storeId];
        }

        var address = await _ethereumService.GenerateNewAddressAsync();
        _storeWallets[storeId] = address;
        
        _logger.LogInformation("Created Ethereum wallet for store {StoreId}: {Address}", 
            storeId, address);
        
        return address;
    }

    public async Task<string> GetDepositAddressAsync(string storeId)
    {
        if (!_storeWallets.TryGetValue(storeId, out var address))
        {
            throw new InvalidOperationException($"No wallet found for store {storeId}");
        }

        return address;
    }

    public async Task<decimal> GetWalletBalanceAsync(string storeId)
    {
        var address = await GetDepositAddressAsync(storeId);
        return await _ethereumService.GetBalanceAsync(address);
    }
}
