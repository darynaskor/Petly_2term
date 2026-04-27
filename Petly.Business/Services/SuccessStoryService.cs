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

    // Отримати список тваринок для випадаючого списку у формі
    public async Task<List<Pet>> GetAvailablePetsAsync()
    {
        return await _context.Pets.ToListAsync();
    }
}