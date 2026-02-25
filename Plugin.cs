using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Abstractions.Models;
using BTCPayServer.Abstractions.Services;
using BTCPayServer.Plugins.EthereumPayments.PaymentMethods;
using BTCPayServer.Plugins.EthereumPayments.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.EthereumPayments;

public class Plugin : BaseBTCPayServerPlugin
{
    public override string Identifier => "BTCPayServer.Plugins.EthereumPayments";
    public override string Name => "Ethereum Payments";
    public override string Description => "Accept ETH and ERC-20 tokens (USDT, dEURO) on Ethereum Mainnet";
    public override Version Version => new Version(1, 0, 0);

    public override void Execute(IServiceCollection services)
    {
        // Register Services
        services.AddSingleton<EthereumRpcService>();
        services.AddSingleton<EthereumPaymentScanner>();
        services.AddHostedService<EthereumTransactionWatcher>();

        // Register Payment Method Handlers
        services.AddSingleton<IPaymentMethodHandler, EthereumPaymentMethodHandler>();
        services.AddSingleton<IPaymentMethodHandler, Erc20PaymentMethodHandler>();

        // Register UI Controllers
        services.AddSingleton<IUIExtension>(new UIExtension(
            "EthereumPayments/UIEthereumSettings",
            "store-integrations-nav"
        ));

        base.Execute(services);
    }
}
