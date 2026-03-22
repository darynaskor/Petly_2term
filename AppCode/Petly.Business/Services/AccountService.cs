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

    public async Task<List<UserProfile>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Account)
            .OrderBy(u => u.AccountId)
            .ToListAsync();
    }

    public async Task<UserProfile?> GetUserAsync(int accountId)
    {
        return await _context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.AccountId == accountId);
    }

    public async Task UpdateUserAsync(UserEditViewModel model)
    {
        var profile = await _context.Users.Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.AccountId == model.AccountId);

        if (profile == null || profile.Account == null)
        {
            throw new InvalidOperationException("Користувача не знайдено");
        }

        profile.Name = model.Name;
        profile.Surname = model.Surname;
        profile.Status = model.Status;

        profile.Account.Email = model.Email;
        profile.Account.Role = model.Role;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            profile.Account.Password = model.Password;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int accountId)
    {
        var profile = await _context.Users.Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.AccountId == accountId);

        if (profile != null)
        {
            _context.Users.Remove(profile);
            if (profile.Account != null)
            {
                _context.Accounts.Remove(profile.Account);
            }
            await _context.SaveChangesAsync();
        }
    }
}
