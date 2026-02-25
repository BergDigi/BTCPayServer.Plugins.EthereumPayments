using BTCPayServer.Payments;

namespace BTCPayServer.Plugins.EthereumPayments.PaymentMethods;

public class EthereumPaymentType
{
    public static readonly PaymentMethodId ETH = new("ETH-OnChain");
    public static readonly PaymentMethodId USDT = new("ETH-USDT-ERC20");
    public static readonly PaymentMethodId DEURO = new("ETH-dEURO-ERC20");

    public static PaymentMethodId[] All => new[] { ETH, USDT, DEURO };
}
