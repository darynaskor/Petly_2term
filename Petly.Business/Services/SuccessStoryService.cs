using Microsoft.EntityFrameworkCore;
using Petly.DataAccess.Data;
using Petly.Models;

namespace Petly.Business.Services;

public class SuccessStoryService
{
    private readonly ApplicationDbContext _context;

    public SuccessStoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Отримати всі історії разом з даними про тваринок
    public async Task<List<SuccessStory>> GetAllStoriesAsync()
    {
        return await _context.SuccessStories
            .Include(s => s.Pet) // Підтягуємо інфу про тваринку
            .OrderByDescending(s => s.CreatedAt) // Свіжі історії зверху
            .ToListAsync();
    }

    // Зберегти нову історію
    public async Task CreateStoryAsync(SuccessStory story)
    {
        _context.SuccessStories.Add(story);
        await _context.SaveChangesAsync();
    }

    // Отримуємо тільки тих тварин, які вже усиновлені
    public async Task<List<Pet>> GetAvailablePetsAsync()
    {
        // Припускаємо, що статус усиновленої тваринки - "Adopted"
        // (Якщо у вас інше слово, наприклад "Усиновлений", замініть його тут)
        return await _context.Pets
            .Where(p => p.Status == "adopted") 
            .ToListAsync();
    }

    
}