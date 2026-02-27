using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BTCPayServer.Plugins.EthereumPayments.Services;

public class EthereumBlockchainMonitor : BackgroundService
{
    private readonly IEthereumService _ethereumService;
    private readonly ILogger<EthereumBlockchainMonitor> _logger;

    public EthereumBlockchainMonitor(
        IEthereumService ethereumService,
        ILogger<EthereumBlockchainMonitor> logger)
    {
        _ethereumService = ethereumService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ethereum Blockchain Monitor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var blockNumber = await _ethereumService.GetCurrentBlockNumberAsync();
                _logger.LogDebug("Current Ethereum block: {BlockNumber}", blockNumber);
                
                // TODO: Check for pending invoices and monitor transactions
                
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Ethereum blockchain monitor");
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
        
        _logger.LogInformation("Ethereum Blockchain Monitor stopped");
    }
}
