using Microsoft.EntityFrameworkCore;
using Petly.DataAccess.Data;
using Petly.Models;

namespace Petly.Business.Services;

public class PetService
{
    private readonly ApplicationDbContext _context;

    public PetService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Метод для отримання всіх тварин
    public async Task<List<Pet>> GetAllPetsAsync()
    {
        return await _context.Pets.ToListAsync();
    }

    // Метод для фільтрації за типом (Кіт/Собака)
    public async Task<List<Pet>> GetPetsByTypeAsync(string type)
    {
        return await _context.Pets
            .Where(p => p.Type == type)
            .ToListAsync();
    }
}