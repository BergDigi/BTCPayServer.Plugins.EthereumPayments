using BTCPayServer.Abstractions.Constants;
using BTCPayServer.Abstractions.Extensions;
using BTCPayServer.Client;
using BTCPayServer.Data;
using BTCPayServer.Plugins.EthereumPayments.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.EthereumPayments.Controllers;

[Route("stores/{storeId}/plugins/ethereum")]
[Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Cookie)]
public class UIEthereumSettingsController : Controller
{
    private readonly StoreRepository _storeRepository;

    public UIEthereumSettingsController(StoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> StoreSettings(string storeId)
    {
        var store = await _storeRepository.FindStore(storeId);
        if (store == null)
            return NotFound();

        var blob = store.GetStoreBlob();
        var settings = blob.GetAdditionalData<EthereumSettings>("Ethereum") ?? new EthereumSettings();

        return View(settings);
    }

    [HttpPost("settings")]
    public async Task<IActionResult> StoreSettings(string storeId, EthereumSettings settings)
    {
        var store = await _storeRepository.FindStore(storeId);
        if (store == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(settings);

        // Validate Ethereum address format (basic check)
        if (!string.IsNullOrEmpty(settings.ReceivingAddress) && 
            !settings.ReceivingAddress.StartsWith("0x") || 
            settings.ReceivingAddress.Length != 42)
        {
            ModelState.AddModelError(nameof(settings.ReceivingAddress), 
                "Invalid Ethereum address format");
            return View(settings);
        }

        var blob = store.GetStoreBlob();
        blob.SetAdditionalData("Ethereum", settings);
        store.SetStoreBlob(blob);

        await _storeRepository.UpdateStore(store);

        TempData[WellKnownTempData.SuccessMessage] = "Ethereum settings updated successfully";
        return RedirectToAction(nameof(StoreSettings), new { storeId });
    }
}
