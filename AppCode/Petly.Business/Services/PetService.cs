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

    // Отримання всіх тварин
    public async Task<List<Pet>> GetAllPetsAsync()
    {
        return await _context.Pets.ToListAsync();
    }

    // Фільтрація та пошук за PetName і Type
    public async Task<List<Pet>> GetPetsAsync(string typeFilter = null, string searchTerm = null)
    {
        var pets = _context.Pets.AsQueryable();

        if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "Усі")
        {
            pets = pets.Where(p => p.Type == typeFilter);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            pets = pets.Where(p => p.PetName.Contains(searchTerm));
        }

        return await pets.ToListAsync();
    }
}