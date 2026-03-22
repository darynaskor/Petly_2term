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

    // Отримати всі тварини (для system_admin)
    public async Task<List<Pet>> GetAllPetsAsync()
    {
        return await _context.Pets.ToListAsync();
    }

    // Отримати тварин конкретного притулку
    public async Task<List<Pet>> GetPetsByShelterAsync(int shelterId)
    {
        return await _context.Pets
            .Where(p => p.ShelterId == shelterId)
            .ToListAsync();
    }

    // Пошук та фільтрація за типом і ім'ям
    public async Task<List<Pet>> GetPetsAsync(string? typeFilter = null, string? searchTerm = null)
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

    public async Task<Pet?> GetPetAsync(int petId)
    {
        return await _context.Pets.FirstOrDefaultAsync(p => p.PetId == petId);
    }

    public async Task AddPetAsync(Pet pet)
    {
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePetAsync(Pet pet)
    {
        _context.Pets.Update(pet);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePetAsync(int petId)
    {
        var pet = await GetPetAsync(petId);
        if (pet != null)
        {
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
        }
    }

    // Для звичайних користувачів — лише доступні тварини
    public async Task<List<Pet>> GetAvailablePetsAsync()
    {
        return await _context.Pets
            .Where(p => p.Status == "Available")
            .ToListAsync();
    }
}