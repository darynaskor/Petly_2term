using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Petly.Business.Services;
using Petly.Models;

namespace Petly.Controllers;

[Authorize]
public class PetsController : Controller
{
    private readonly PetService _petService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PetsController(PetService petService, UserManager<ApplicationUser> userManager)
    {
        _petService = petService;
        _userManager = userManager;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(User);
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await GetCurrentUserAsync();
        var role = currentUser != null ? (await _userManager.GetRolesAsync(currentUser)).FirstOrDefault() ?? "user" : "user";
        var accountId = currentUser?.Id;

        List<Pet> pets;
        if (role == "shelter_admin" && accountId != null)
        {
            pets = await _petService.GetPetsByShelterAsync(accountId.Value);
        }
        else
        {
            pets = await _petService.GetAllPetsAsync();
        }

        ViewBag.Role = role;
        ViewBag.AccountId = accountId;

        return View(pets);
    }

    public async Task<IActionResult> Details(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null) return NotFound();
        return View(pet);
    }

    [Authorize(Roles = "shelter_admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "shelter_admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pet pet)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null || !User.IsInRole("shelter_admin"))
        {
            return Forbid();
        }

        pet.ShelterId = currentUser.Id;

        if (!ModelState.IsValid) return View(pet);

        await _petService.AddPetAsync(pet);
        TempData["Success"] = "Тварину додано";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "shelter_admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null)
        {
            return NotFound();
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null || !User.IsInRole("shelter_admin"))
        {
            return Forbid();
        }

        if (pet.ShelterId != currentUser.Id) return Forbid();

        return View(pet);
    }

    [HttpPost]
[Authorize(Roles = "shelter_admin")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(Pet pet)
{
    var currentUser = await GetCurrentUserAsync();
    if (currentUser == null || !User.IsInRole("shelter_admin")) return Forbid();

    var existingPet = await _petService.GetPetAsync(pet.PetId);
    if (existingPet == null) return NotFound();
    if (existingPet.ShelterId != currentUser.Id) return Forbid();

    if (!ModelState.IsValid) return View(pet);
    
    existingPet.PetName = pet.PetName;
    existingPet.Type = pet.Type;
    existingPet.Breed = pet.Breed;
    existingPet.Gender = pet.Gender;
    existingPet.Age = pet.Age;
    existingPet.Size = pet.Size;
    existingPet.Vaccinated = pet.Vaccinated;
    existingPet.Sterilized = pet.Sterilized;
    existingPet.Description = pet.Description;

    await _petService.UpdatePetAsync(existingPet);
    TempData["Success"] = "Дані тварини оновлено";

    return RedirectToAction(nameof(Index));
}

    [Authorize(Roles = "shelter_admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null) return NotFound();

        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null || !User.IsInRole("shelter_admin")) return Forbid();
        if (pet.ShelterId != currentUser.Id) return Forbid();

        return View(pet);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "shelter_admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null) return NotFound();

        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null || !User.IsInRole("shelter_admin")) return Forbid();
        if (pet.ShelterId != currentUser.Id) return Forbid();

        await _petService.DeletePetAsync(id);
        TempData["Success"] = "Тварину видалено";
        return RedirectToAction(nameof(Index));
    }
}
