using Microsoft.AspNetCore.Mvc;
using Petly.Business.Services;
using Petly.Models;

namespace Petly.Controllers;

public class NeedsController : Controller
{
    private readonly NeedService _needService;

    public NeedsController(NeedService needService)
    {
        _needService = needService;
    }

    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("Role");
        var accountId = HttpContext.Session.GetInt32("AccountId");

        ViewBag.CanCreate = role == "shelter_admin";
        ViewBag.UserRole = role;

        var needs = await _needService.GetNeedsAsync(accountId, role);
        return View(needs);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!CanCreateOrManage())
        {
            return Forbid();
        }

        return View(new ShelterNeedFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShelterNeedFormViewModel model)
    {
        if (!CanCreateOrManage())
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return Forbid();
        }

        var need = new ShelterNeed
        {
            ShelterId = accountId.Value,
            Description = model.Description.Trim(),
            PaymentDetails = model.PaymentDetails.Trim()
        };

        await _needService.AddNeedAsync(need);
        TempData["Success"] = "Потребу успішно додано.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var need = await _needService.GetNeedAsync(id);
        if (need == null)
        {
            return NotFound();
        }

        if (!CanManageNeed(need))
        {
            return Forbid();
        }

        var model = new ShelterNeedFormViewModel
        {
            NeedId = need.NeedId,
            Description = need.Description,
            PaymentDetails = need.PaymentDetails
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ShelterNeedFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var need = await _needService.GetNeedAsync(model.NeedId);
        if (need == null)
        {
            return NotFound();
        }

        if (!CanManageNeed(need))
        {
            return Forbid();
        }

        need.Description = model.Description.Trim();
        need.PaymentDetails = model.PaymentDetails.Trim();

        await _needService.UpdateNeedAsync(need);
        TempData["Success"] = "Потребу оновлено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var need = await _needService.GetNeedAsync(id);
        if (need == null)
        {
            return NotFound();
        }

        if (!CanManageNeed(need))
        {
            return Forbid();
        }

        return View(need);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var need = await _needService.GetNeedAsync(id);
        if (need == null)
        {
            return NotFound();
        }

        if (!CanManageNeed(need))
        {
            return Forbid();
        }

        await _needService.DeleteNeedAsync(id);
        TempData["Success"] = "Потребу видалено.";
        return RedirectToAction(nameof(Index));
    }

    private bool CanCreateOrManage()
    {
        var role = HttpContext.Session.GetString("Role");
        return role == "shelter_admin";
    }

    private bool CanManageNeed(ShelterNeed need)
    {
        var role = HttpContext.Session.GetString("Role");
        var accountId = HttpContext.Session.GetInt32("AccountId");

        return role == "system_admin" || (role == "shelter_admin" && accountId == need.ShelterId);
    }
}
