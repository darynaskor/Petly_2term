using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Petly.Business.Services;

namespace Petly.Controllers;

public class HomeController : Controller
{
    private readonly PetService _petService;

    // Впроваджуємо сервіс через конструктор
    public HomeController(PetService petService)
    {
        _petService = petService;
    }

    public async Task<IActionResult> Index()
    {
        // Отримуємо список усіх тварин з бази
        var pets = await _petService.GetAllPetsAsync();
        return View(pets);
    }
}