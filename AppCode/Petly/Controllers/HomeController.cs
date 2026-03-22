using Microsoft.AspNetCore.Mvc;
using Petly.Business.Services;

namespace Petly.Controllers;

public class HomeController : Controller
{
    private readonly PetService _petService;

    public HomeController(PetService petService)
    {
        _petService = petService;
    }

    public async Task<IActionResult> Index(string typeFilter, string searchTerm)
    {
        var pets = await _petService.GetPetsAsync(typeFilter, searchTerm);
        ViewBag.TypeFilter = typeFilter ?? "Усі";
        ViewBag.SearchTerm = searchTerm ?? "";
        return View(pets);
    }

    public IActionResult About()
{
    return View();
}

public IActionResult Needs()
{
    return RedirectToAction("Index", "Needs");
}

public IActionResult Adoption()
{
    return View(); // поки порожня
}
}
