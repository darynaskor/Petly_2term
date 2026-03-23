using Microsoft.EntityFrameworkCore;
using Petly.DataAccess.Data;
using Petly.Models;

namespace Petly.Business.Services;

public class AccountService
{
    private readonly ApplicationDbContext _context;

    public AccountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByCredentialsAsync(string email, string password)
    {
        return await _context.Accounts
            .Include(a => a.UserProfile)
            .FirstOrDefaultAsync(a => a.Email == email && a.Password == password);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Accounts.AnyAsync(a => a.Email == email && (!excludeId.HasValue || a.Id != excludeId));
    }

    public async Task<UserProfile> CreateUserAsync(RegisterViewModel model)
    {
        var account = new Account
        {
            Email = model.Email,
            Password = model.Password,
            Role = "user",
            RegistrationDate = DateTime.Now
        };

        var profile = new UserProfile
        {
            Name = model.Name,
            Surname = model.Surname,
            Status = "Активний",
            Account = account
        };

        _context.Accounts.Add(account);
        _context.Users.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<List<AdminUserViewModel>> GetAllUsersAsync()
    {
        var accounts = await _context.Accounts
            .Include(a => a.UserProfile)
            .Where(a => a.Role == "user" || a.Role == "shelter_admin")
            .OrderBy(a => a.Id)
            .ToListAsync();

        var shelters = await _context.Shelters.ToDictionaryAsync(s => s.AccountId);

        return accounts.Select(a =>
        {
            shelters.TryGetValue(a.Id, out var shelter);
            var isShelterAdmin = a.Role == "shelter_admin";

            return new AdminUserViewModel
            {
                AccountId = a.Id,
                Name = isShelterAdmin ? shelter?.AdminName ?? shelter?.ShelterName : a.UserProfile?.Name,
                Surname = isShelterAdmin ? null : a.UserProfile?.Surname,
                Email = a.Email,
                Role = a.Role,
                Status = isShelterAdmin ? "Активний" : a.UserProfile?.Status ?? "Активний",
                RegistrationDate = a.RegistrationDate,
                ShelterName = shelter?.ShelterName
            };
        }).ToList();
    }

    public async Task<UserEditViewModel?> GetUserAsync(int accountId)
    {
        var account = await _context.Accounts
            .Include(a => a.UserProfile)
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null) return null;

        var shelter = await _context.Shelters.FirstOrDefaultAsync(s => s.AccountId == accountId);

        return new UserEditViewModel
        {
            AccountId = account.Id,
            Name = account.UserProfile?.Name ?? shelter?.AdminName ?? shelter?.ShelterName ?? string.Empty,
            Surname = account.UserProfile?.Surname ?? string.Empty,
            Email = account.Email,
            Role = account.Role,
            Status = account.UserProfile?.Status ?? "Активний",
            Password = string.Empty
        };
    }

    public async Task UpdateUserAsync(UserEditViewModel model)
    {
        var account = await _context.Accounts
            .Include(a => a.UserProfile)
            .FirstOrDefaultAsync(a => a.Id == model.AccountId);

        if (account == null)
            throw new InvalidOperationException("Акаунт не знайдено");

        account.Email = model.Email;
        account.Role = model.Role;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            account.Password = model.Password;
        }

        if (account.UserProfile != null)
        {
            account.UserProfile.Name = model.Name;
            account.UserProfile.Surname = model.Surname;
            account.UserProfile.Status = model.Status;
        }
        else
        {
            var shelter = await _context.Shelters.FirstOrDefaultAsync(s => s.AccountId == model.AccountId);

            if (shelter != null)
            {
                shelter.AdminName = model.Name;
            }
            else if (account.Role == "user")
            {
                // Якщо роль змінено на user для акаунта без профілю — створюємо його.
                var profile = new UserProfile
                {
                    AccountId = account.Id,
                    Name = model.Name,
                    Surname = model.Surname,
                    Status = model.Status
                };
                _context.Users.Add(profile);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int accountId)
    {
        var profile = await _context.Users.Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.AccountId == accountId);

        var shelter = await _context.Shelters.FirstOrDefaultAsync(s => s.AccountId == accountId);

        if (shelter != null)
        {
            var pets = await _context.Pets.Where(p => p.ShelterId == accountId).ToListAsync();
            var needs = await _context.ShelterNeeds.Where(n => n.ShelterId == accountId).ToListAsync();

            if (needs.Any()) _context.ShelterNeeds.RemoveRange(needs);
            if (pets.Any()) _context.Pets.RemoveRange(pets);

            _context.Shelters.Remove(shelter);
        }

        if (profile != null)
        {
            _context.Users.Remove(profile);
        }

        var account = profile?.Account ?? await _context.Accounts.FindAsync(accountId);
        if (account != null)
        {
            _context.Accounts.Remove(account);
        }

        await _context.SaveChangesAsync();
    }
}
