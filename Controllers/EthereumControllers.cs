using System.Threading.Tasks;
using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Plugins.EthereumPayments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.EthereumPayments.Controllers;

[Route("plugins/ethereum")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.Cookie)]
public class EthereumController : Controller
{
    private readonly IEthereumService _ethereumService;
    private readonly IEthereumWalletService _walletService;

    public EthereumController(
        IEthereumService ethereumService,
        IEthereumWalletService walletService)
    {
        _ethereumService = ethereumService;
        _walletService = walletService;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Plugins/BTCPayServer.Plugins.EthereumPayments/Views/Ethereum/Index.cshtml");
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var blockNumber = await _ethereumService.GetCurrentBlockNumberAsync();
        return Json(new { blockNumber, status = "connected" });
    }
}
