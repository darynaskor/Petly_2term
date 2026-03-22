using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Petly.Business.Services;
using Petly.DataAccess.Data;
using Petly.Models;

namespace Petly.Controllers;

public class PetsController : Controller
{
    private readonly PetService _petService;

    public PetsController(PetService petService)
    {
        _petService = petService;
    }

    // GET: /Pets
    public async Task<IActionResult> Index()
    {
        string? role = HttpContext.Session.GetString("Role");
        int? accountId = HttpContext.Session.GetInt32("AccountId");

        if (role == "shelter_admin" && accountId.HasValue)
        {
            var pets = await _petService.GetPetsByShelterAsync(accountId.Value);
            return View(pets);
        }
        else if (role == "system_admin")
        {
            var pets = await _petService.GetAllPetsAsync();
            return View(pets);
        }
        else
        {
            var pets = await _petService.GetAvailablePetsAsync();
            return View(pets);
        }
    }

    // GET: /Pets/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null) return NotFound();
        return View(pet);
    }

    // GET: /Pets/Create
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "shelter_admin") return Forbid();
        return View();
    }

    // POST: /Pets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pet pet)
    {
        if (HttpContext.Session.GetString("Role") != "shelter_admin") return Forbid();
        if (!ModelState.IsValid) return View(pet);

        pet.ShelterId = HttpContext.Session.GetInt32("AccountId") ?? 0;
        await _petService.AddPetAsync(pet);
        TempData["Success"] = "Тварину додано!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Pets/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null) return NotFound();

        if (HttpContext.Session.GetString("Role") == "shelter_admin" &&
            pet.ShelterId != (HttpContext.Session.GetInt32("AccountId") ?? 0))
        {
            return Forbid();
        }

        return View(pet);
    }

    // POST: /Pets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Pet pet)
    {
        if (!ModelState.IsValid) return View(pet);

        var existingPet = await _petService.GetPetAsync(pet.PetId);
        if (existingPet == null) return NotFound();

        if (HttpContext.Session.GetString("Role") == "shelter_admin" &&
            existingPet.ShelterId != (HttpContext.Session.GetInt32("AccountId") ?? 0))
        {
            return Forbid();
        }

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
        TempData["Success"] = "Дані тварини оновлено!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Pets/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null) return NotFound();

        if (HttpContext.Session.GetString("Role") == "shelter_admin" &&
            pet.ShelterId != (HttpContext.Session.GetInt32("AccountId") ?? 0))
        {
            return Forbid();
        }

        return View(pet);
    }

    // POST: /Pets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pet = await _petService.GetPetAsync(id);
        if (pet == null) return NotFound();

        if (HttpContext.Session.GetString("Role") == "shelter_admin" &&
            pet.ShelterId != (HttpContext.Session.GetInt32("AccountId") ?? 0))
        {
            return Forbid();
        }

        await _petService.DeletePetAsync(id);
        TempData["Success"] = "Тварину видалено!";
        return RedirectToAction(nameof(Index));
    }
}