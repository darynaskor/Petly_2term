using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Petly.Business.Services;
using Petly.Controllers;
using Petly.DataAccess.Data;
using Petly.Models;
using Xunit;

namespace Petly.Tests;

public class AccountControllerAdminTests
{
    [Fact]
    public async Task LoginSuccess()
    {
        await using var db = CreateDbContext();
        db.Accounts.Add(new Account
        {
            Id = 10,
            Email = "admin@petly.com",
            Password = "secret",
            Role = "system_admin",
            UserProfile = new UserProfile
            {
                AccountId = 10,
                Name = "Admin",
                Surname = "System",
                Status = "Активний"
            }
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var model = new LoginViewModel
        {
            Email = "admin@petly.com",
            Password = "secret"
        };

        var result = await controller.Login(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        Assert.Equal(10, controller.HttpContext.Session.GetInt32("AccountId"));
        Assert.Equal("system_admin", controller.HttpContext.Session.GetString("Role"));
        Assert.Equal("admin@petly.com", controller.HttpContext.Session.GetString("UserEmail"));
        Assert.Equal("Привіт, Admin!", controller.TempData["Success"]);
    }

    [Fact]
    public async Task LoginWrongPassword()
    {
        await using var db = CreateDbContext();
        db.Accounts.Add(new Account
        {
            Id = 11,
            Email = "admin@petly.com",
            Password = "secret",
            Role = "system_admin"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var model = new LoginViewModel
        {
            Email = "admin@petly.com",
            Password = "wrong-password"
        };

        var result = await controller.Login(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
        var error = Assert.Single(controller.ModelState[string.Empty].Errors);
        Assert.Equal("Неправильний email або пароль", error.ErrorMessage);
        Assert.Null(controller.HttpContext.Session.GetInt32("AccountId"));
    }

    [Fact]
    public async Task ViewUsersList()
    {
        await using var db = CreateDbContext();
        db.Accounts.AddRange(
            new Account
            {
                Id = 1,
                Email = "user@petly.com",
                Password = "pass",
                Role = "user",
                RegistrationDate = new DateTime(2026, 3, 24),
                UserProfile = new UserProfile
                {
                    AccountId = 1,
                    Name = "Ira",
                    Surname = "User",
                    Status = "Активний"
                }
            },
            new Account
            {
                Id = 2,
                Email = "shelter@petly.com",
                Password = "pass",
                Role = "shelter_admin",
                RegistrationDate = new DateTime(2026, 3, 23)
            });
        db.Shelters.Add(new Shelter
        {
            AccountId = 2,
            ShelterName = "Pet House",
            AdminName = "Oleh",
            Location = "Lviv"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "system_admin");

        var result = await controller.Users();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<AdminUserViewModel>>(view.Model);
        Assert.Equal(2, model.Count);
        Assert.Contains(model, x => x.Email == "user@petly.com" && x.Name == "Ira" && x.Role == "user");
        Assert.Contains(model, x => x.Email == "shelter@petly.com" && x.Name == "Oleh" && x.ShelterName == "Pet House");
    }

    [Fact]
    public async Task ViewUsersListWithoutAccess()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "user");

        var result = await controller.Users();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task OpenEditPage()
    {
        await using var db = CreateDbContext();
        db.Accounts.Add(new Account
        {
            Id = 5,
            Email = "member@petly.com",
            Password = "pass",
            Role = "user",
            UserProfile = new UserProfile
            {
                AccountId = 5,
                Name = "Nazar",
                Surname = "Test",
                Status = "Активний"
            }
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "system_admin");

        var result = await controller.Edit(5);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserEditViewModel>(view.Model);
        Assert.Equal(5, model.AccountId);
        Assert.Equal("member@petly.com", model.Email);
        Assert.Equal("Nazar", model.Name);
        Assert.Equal("Test", model.Surname);
    }

    [Fact]
    public async Task OpenEditPageUserNotFound()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "system_admin");

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateUser()
    {
        await using var db = CreateDbContext();
        db.Accounts.Add(new Account
        {
            Id = 7,
            Email = "old@petly.com",
            Password = "oldpass",
            Role = "user",
            UserProfile = new UserProfile
            {
                AccountId = 7,
                Name = "Old",
                Surname = "Name",
                Status = "Активний"
            }
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "system_admin");
        var model = new UserEditViewModel
        {
            AccountId = 7,
            Email = "new@petly.com",
            Name = "Updated",
            Surname = "User",
            Status = "Заблокований",
            Role = "user",
            Password = "newpass"
        };

        var result = await controller.Edit(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirect.ActionName);
        Assert.Equal("Дані користувача оновлено", controller.TempData["Success"]);

        var updated = await db.Accounts.Include(a => a.UserProfile).SingleAsync(a => a.Id == 7);
        Assert.Equal("new@petly.com", updated.Email);
        Assert.Equal("newpass", updated.Password);
        Assert.NotNull(updated.UserProfile);
        Assert.Equal("Updated", updated.UserProfile!.Name);
        Assert.Equal("User", updated.UserProfile.Surname);
        Assert.Equal("Заблокований", updated.UserProfile.Status);
    }

    [Fact]
    public async Task UpdateUserDuplicateEmail()
    {
        await using var db = CreateDbContext();
        db.Accounts.AddRange(
            new Account
            {
                Id = 20,
                Email = "first@petly.com",
                Password = "pass",
                Role = "user",
                UserProfile = new UserProfile
                {
                    AccountId = 20,
                    Name = "First",
                    Surname = "User",
                    Status = "Активний"
                }
            },
            new Account
            {
                Id = 21,
                Email = "second@petly.com",
                Password = "pass",
                Role = "user",
                UserProfile = new UserProfile
                {
                    AccountId = 21,
                    Name = "Second",
                    Surname = "User",
                    Status = "Активний"
                }
            });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "system_admin");
        var model = new UserEditViewModel
        {
            AccountId = 21,
            Email = "first@petly.com",
            Name = "Second",
            Surname = "User",
            Status = "Активний",
            Role = "user"
        };

        var result = await controller.Edit(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
        var error = Assert.Single(controller.ModelState["Email"].Errors);
        Assert.Equal("Такий email вже використовується", error.ErrorMessage);

        var unchanged = await db.Accounts.SingleAsync(a => a.Id == 21);
        Assert.Equal("second@petly.com", unchanged.Email);
    }

    [Fact]
    public async Task DeleteShelterAccount()
    {
        await using var db = CreateDbContext();
        db.Accounts.Add(new Account
        {
            Id = 9,
            Email = "shelter-admin@petly.com",
            Password = "pass",
            Role = "shelter_admin"
        });
        db.Shelters.Add(new Shelter
        {
            AccountId = 9,
            ShelterName = "Safe Paw",
            AdminName = "Oksana",
            Location = "Kyiv"
        });
        db.Pets.Add(new Pet
        {
            PetId = 101,
            ShelterId = 9,
            PetName = "Barsik"
        });
        db.ShelterNeeds.Add(new ShelterNeed
        {
            NeedId = 55,
            ShelterId = 9,
            Description = "Food",
            PaymentDetails = "Card"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "system_admin");

        var result = await controller.Delete(9);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Users", redirect.ActionName);
        Assert.Equal("Користувача видалено", controller.TempData["Success"]);
        Assert.False(await db.Accounts.AnyAsync(a => a.Id == 9));
        Assert.False(await db.Shelters.AnyAsync(s => s.AccountId == 9));
        Assert.False(await db.Pets.AnyAsync(p => p.ShelterId == 9));
        Assert.False(await db.ShelterNeeds.AnyAsync(n => n.ShelterId == 9));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AccountController CreateController(ApplicationDbContext db, string? role = null)
    {
        var service = new AccountService(db);
        var controller = new AccountController(service);
        var httpContext = new DefaultHttpContext
        {
            Session = new TestSession()
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            httpContext.Session.SetString("Role", role);
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[]? value) => _store.TryGetValue(key, out value);
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
