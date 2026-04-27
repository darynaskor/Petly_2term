using Microsoft.AspNetCore.Mvc;
using Petly.Business.Services; // Підключаємо папку з сервісами
using Petly.Models;

namespace Petly.Controllers;

public class SuccessStoriesController : Controller
{
    private readonly SuccessStoryService _storyService;

    // Підключаємо наш новий сервіс
    public SuccessStoriesController(SuccessStoryService storyService)
    {
        _storyService = storyService;
    }

    public async Task<IActionResult> Index()
    {
        var stories = await _storyService.GetAllStoriesAsync();
        return View(stories);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Pets = await _storyService.GetAvailablePetsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SuccessStory story)
    {
        if (ModelState.IsValid)
        {
            await _storyService.CreateStoryAsync(story);
            return RedirectToAction(nameof(Index));
        }
        
        // Якщо помилка, знову завантажуємо список тварин
        ViewBag.Pets = await _storyService.GetAvailablePetsAsync();
        return View(story);
    }
}