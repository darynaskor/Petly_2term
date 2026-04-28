using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petly.Business.Services;
using Petly.Models;
using Microsoft.AspNetCore.Hosting; 

namespace Petly.Controllers;

public class SuccessStoriesController : Controller
{
    private readonly SuccessStoryService _storyService;
    private readonly IWebHostEnvironment _webHostEnvironment; 

    public SuccessStoriesController(SuccessStoryService storyService, IWebHostEnvironment webHostEnvironment)
    {
        _storyService = storyService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var stories = await _storyService.GetAllStoriesAsync();
        return View(stories);
    }

    [Authorize(Roles = "shelter_admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Pets = await _storyService.GetAvailablePetsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "shelter_admin")]
    public async Task<IActionResult> Create(SuccessStory story, IFormFile? uploadFile)
    {
        ModelState.Remove("Pet"); 

        if (ModelState.IsValid)
        {
            if (uploadFile != null && uploadFile.Length > 0)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFile.FileName);
                string uploadsFolder = Path.Combine(wwwRootPath, "images", "success_stories");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadFile.CopyToAsync(fileStream);
                }

                story.ImageUrl = "/images/success_stories/" + fileName;
            }
            await _storyService.CreateStoryAsync(story);
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.Pets = await _storyService.GetAvailablePetsAsync();
        return View(story);
    }
}