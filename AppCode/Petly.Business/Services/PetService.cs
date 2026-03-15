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

    public async Task<List<Pet>> GetAllPetsAsync()
    {
        return await _context.Pets.ToListAsync();
    }

    public async Task<List<Pet>> GetPetsByTypeAsync(string type)
    {
        return await _context.Pets
            .Where(p => p.Type == type)
            .ToListAsync();
    }
}