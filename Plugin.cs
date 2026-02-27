using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.EthereumPayments;

public class EthereumPaymentsPlugin : BaseBTCPayServerPlugin
{
    public override string Identifier => "BTCPayServer.Plugins.EthereumPayments";
    
    public override string Name => "Ethereum Payments";
    
    public override string Description => "Accept Ethereum and ERC-20 tokens (dEURO, USDT) as payment methods";
    
    public override string SystemName => "ethereum-payments";

    public override void Execute(IServiceCollection services)
    {
        // Ethereum Service Registrierung
        services.AddSingleton<Services.IEthereumService, Services.EthereumService>();
        services.AddSingleton<Services.IEthereumWalletService, Services.EthereumWalletService>();
        
        // Payment Handler Registrierung
        services.AddSingleton<Handlers.EthereumPaymentMethodHandler>();
        services.AddSingleton<Handlers.dEUROPaymentMethodHandler>();
        services.AddSingleton<Handlers.USDTPaymentMethodHandler>();
        
        // Hosted Service für Blockchain Monitoring
        services.AddHostedService<Services.EthereumBlockchainMonitor>();
        
        // MVC Services
        services.AddMvc()
            .AddApplicationPart(typeof(EthereumPaymentsPlugin).Assembly)
            .AddControllersAsServices();
    }
}
