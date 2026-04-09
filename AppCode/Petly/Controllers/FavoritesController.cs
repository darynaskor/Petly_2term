using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Petly.DataAccess.Data;
using Petly.Models;

namespace Petly.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(User);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int petId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return RedirectToAction("Login", "Account");

        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == user.Id && f.PetId == petId);

        if (!exists)
        {
            _context.Favorites.Add(new Favorite
            {
                UserId = user.Id,
                PetId = petId
            });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("Role");

        // ❌ ЗАБОРОНА ДЛЯ АДМІНІВ
        if (role == "shelter_admin" || role == "system_admin")
        {
            return Forbid();
        }

        var user = await GetCurrentUserAsync();
        if (user == null) return RedirectToAction("Login", "Account");

        var favorites = await _context.Favorites
            .Include(f => f.Pet)
            .Where(f => f.UserId == user.Id)
            .ToListAsync();

        return View(favorites);
    }

    public async Task<IActionResult> Remove(int id)
    {
        var fav = await _context.Favorites.FindAsync(id);

        if (fav != null)
        {
            _context.Favorites.Remove(fav);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}