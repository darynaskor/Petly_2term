using Microsoft.EntityFrameworkCore;
using Petly.DataAccess.Data; // Використовуємо ваш простір імен
using Petly.Models;

namespace Petly.Business.Services;

public class AdoptionService
{
    private readonly ApplicationDbContext _context;

    public AdoptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Отримати всі заявки конкретного користувача з даними про тварину
    public async Task<List<AdoptionApplication>> GetUserApplicationsAsync(int userId)
    {
        return await _context.AdoptionApplications
            .Include(a => a.Pet) // Щоб працювало @app.Pet?.PetName у View
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.SubmissionDate)
            .ToListAsync();
    }

    // Створити нову заявку
    public async Task CreateApplicationAsync(int petId, int userId)
    {
        var application = new AdoptionApplication
        {
            PetId = petId,
            UserId = userId,
            Status = "Очікує", // Значення за замовчуванням
            SubmissionDate = DateTime.Now
        };

        _context.AdoptionApplications.Add(application);
        await _context.SaveChangesAsync();
    }

    // Отримати заявки для притулку (де ShelterId тварини збігається з ID адміна)
public async Task<List<AdoptionApplication>> GetShelterApplicationsAsync(int shelterAccountId)
{
    return await _context.AdoptionApplications
        .Include(a => a.Pet)
        .Where(a => a.Pet != null && a.Pet.ShelterId == shelterAccountId)
        .OrderByDescending(a => a.SubmissionDate)
        .ToListAsync();
}

// Оновити статус заявки
public async Task UpdateApplicationStatusAsync(int adoptId, string newStatus)
{
    var application = await _context.AdoptionApplications.FindAsync(adoptId);
    if (application != null)
    {
        application.Status = newStatus;
        await _context.SaveChangesAsync();
    }
}

public async Task DeleteApplicationAsync(int adoptId, int userId)
{
    var application = await _context.AdoptionApplications
        .FirstOrDefaultAsync(a => a.AdoptId == adoptId && a.UserId == userId);

    if (application != null)
    {
        _context.AdoptionApplications.Remove(application);
        await _context.SaveChangesAsync();
    }
}
}