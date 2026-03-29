using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Petly.Business.Services;
using Petly.Models;

namespace Petly.Controllers;

public class AdoptionController : Controller
{
    private readonly AdoptionService _adoptionService;
    private readonly PetService _petService;

    public AdoptionController(AdoptionService adoptionService, PetService petService)
    {
        _adoptionService = adoptionService;
        _petService = petService;
    }

    public async Task<IActionResult> Index()
    {
        string? role = HttpContext.Session.GetString("Role");
        int? accountId = HttpContext.Session.GetInt32("AccountId");

        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }


        if (role == "shelter_admin")
        {
            var applications = await _adoptionService.GetShelterApplicationsAsync(accountId.Value);
            ViewBag.IsAdmin = true;
            return View("Adoption", applications);
        }
        else
        {
            var applications = await _adoptionService.GetUserApplicationsAsync(accountId.Value);
            ViewBag.IsAdmin = false;
            return View("Adoption", applications);
        }
    }

    public async Task<IActionResult> Create(int petId)
    {
        var pet = await _petService.GetPetAsync(petId);
        if (pet == null) return NotFound();

        return View("Adopt", pet);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adopt(int petId)
    {
        int? accountId = HttpContext.Session.GetInt32("AccountId");
        
        if (!accountId.HasValue) return RedirectToAction("Login", "Account");

        await _adoptionService.CreateApplicationAsync(petId, accountId.Value);
        
        // Додаємо TempData як у PetsController
        TempData["Success"] = "Вашу заявку успішно подано!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int adoptId)
    {
        if (HttpContext.Session.GetString("Role") != "shelter_admin") return Forbid();

        await _adoptionService.UpdateApplicationStatusAsync(adoptId, "Схвалено");
        TempData["Success"] = "Заявку схвалено!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int adoptId)
    {
        if (HttpContext.Session.GetString("Role") != "shelter_admin") return Forbid();

        await _adoptionService.UpdateApplicationStatusAsync(adoptId, "Відхилено");
        TempData["Success"] = "Заявку відхилено.";
        return RedirectToAction(nameof(Index));
    }
    // POST: /Adoption/Delete/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int adoptId)
{
    int? accountId = HttpContext.Session.GetInt32("AccountId"); 

    if (!accountId.HasValue) return RedirectToAction("Login", "Account");

    await _adoptionService.DeleteApplicationAsync(adoptId, accountId.Value);
    
    TempData["Success"] = "Вашу заявку успішно скасовано.";
    return RedirectToAction(nameof(Index));
}
}
