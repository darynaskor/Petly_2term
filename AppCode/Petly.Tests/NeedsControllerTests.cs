using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Petly.Business.Services;
using Petly.Controllers;
using Petly.DataAccess.Data;
using Petly.Models;
using Xunit;

namespace Petly.Tests;

public class NeedsControllerTests
{
    [Fact]
    public async Task ViewNeedsList()
    {
        await using var db = CreateDbContext();
        db.Shelters.AddRange(
            new Shelter
            {
                AccountId = 5,
                ShelterName = "Shelter Alpha",
                Location = "Kyiv",
                AdminName = "Ira"
            },
            new Shelter
            {
                AccountId = 6,
                ShelterName = "Shelter Beta",
                Location = "Lviv",
                AdminName = "Oleh"
            });
        db.ShelterNeeds.AddRange(
            new ShelterNeed
            {
                NeedId = 1,
                ShelterId = 5,
                Description = "Потрібен корм для котів",
                PaymentDetails = "Можна привезти в притулок"
            },
            new ShelterNeed
            {
                NeedId = 3,
                ShelterId = 5,
                Description = "Потрібні миски",
                PaymentDetails = "Передати особисто"
            },
            new ShelterNeed
            {
                NeedId = 2,
                ShelterId = 6,
                Description = "Потрібні ковдри",
                PaymentDetails = "Переказ на рахунок"
            });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<ShelterNeedGroupViewModel>>(view.Model);
        Assert.Equal(2, model.Count);
        Assert.True((bool)controller.ViewBag.CanCreate);
        Assert.Equal("shelter_admin", controller.ViewBag.UserRole);

        var firstShelter = Assert.Single(model, x => x.ShelterId == 5);
        Assert.True(firstShelter.CanManage);
        Assert.Equal("Київ", firstShelter.Location);
        Assert.Equal(2, firstShelter.Needs.Count);
        Assert.Contains(firstShelter.Needs, x => x.NeedId == 1 && x.Description == "Потрібен корм для котів");
        Assert.Contains(firstShelter.Needs, x => x.NeedId == 3 && x.PaymentDetails == "Передати особисто");

        var secondShelter = Assert.Single(model, x => x.ShelterId == 6);
        Assert.False(secondShelter.CanManage);
        Assert.Equal("Львів", secondShelter.Location);
        Assert.Single(secondShelter.Needs);
        Assert.Contains(secondShelter.Needs, x => x.NeedId == 2);
    }

    [Fact]
    public async Task OpenCreatePage()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Create(5);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ShelterNeedFormViewModel>(view.Model);
        Assert.Equal(5, model.ShelterId);
    }

    [Fact]
    public async Task OpenCreatePageWithoutAccess()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "user", 5);

        var result = await controller.Create(5);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task OpenCreatePageWithAnotherShelterIdReturnsForbid()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Create(6);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task CreateNeed()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "shelter_admin", 7);
        var model = new ShelterNeedFormViewModel
        {
            ShelterId = 7,
            Description = "  Потрібні ліки для тварин  ",
            PaymentDetails = "  Оплата на картку  "
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Потребу успішно додано.", controller.TempData["Success"]);

        var saved = await db.ShelterNeeds.SingleAsync();
        Assert.Equal(7, saved.ShelterId);
        Assert.Equal("Потрібні ліки для тварин", saved.Description);
        Assert.Equal("Оплата на картку", saved.PaymentDetails);
    }

    [Fact]
    public async Task OpenEditPage()
    {
        await using var db = CreateDbContext();
        db.Shelters.Add(new Shelter
        {
            AccountId = 4,
            ShelterName = "Shelter Delta",
            Location = "Dnipro",
            AdminName = "Marta"
        });
        db.ShelterNeeds.Add(new ShelterNeed
        {
            NeedId = 10,
            ShelterId = 4,
            Description = "Потрібен наповнювач",
            PaymentDetails = "Можна передати особисто"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 4);

        var result = await controller.Edit(10);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ShelterNeedFormViewModel>(view.Model);
        Assert.Equal(10, model.NeedId);
        Assert.Equal(4, model.ShelterId);
        Assert.Equal("Потрібен наповнювач", model.Description);
        Assert.Equal("Можна передати особисто", model.PaymentDetails);
    }

    [Fact]
    public async Task OpenEditPageNeedNotFound()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db, "shelter_admin", 4);

        var result = await controller.Edit(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task OpenEditPageWithoutAccess()
    {
        await using var db = CreateDbContext();
        db.Shelters.Add(new Shelter
        {
            AccountId = 8,
            ShelterName = "Shelter Sigma",
            Location = "Odesa",
            AdminName = "Taras"
        });
        db.ShelterNeeds.Add(new ShelterNeed
        {
            NeedId = 11,
            ShelterId = 8,
            Description = "Потрібні переноски",
            PaymentDetails = "Можна надіслати поштою"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 7);

        var result = await controller.Edit(11);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateNeed()
    {
        await using var db = CreateDbContext();
        db.Shelters.Add(new Shelter
        {
            AccountId = 9,
            ShelterName = "Shelter Nova",
            Location = "Kyiv",
            AdminName = "Olha"
        });
        db.ShelterNeeds.Add(new ShelterNeed
        {
            NeedId = 12,
            ShelterId = 9,
            Description = "Старий опис",
            PaymentDetails = "Старі реквізити"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 9);
        var model = new ShelterNeedFormViewModel
        {
            NeedId = 12,
            ShelterId = 9,
            Description = "  Новий опис потреби  ",
            PaymentDetails = "  Нові реквізити  "
        };

        var result = await controller.Edit(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Потребу оновлено.", controller.TempData["Success"]);

        var updated = await db.ShelterNeeds.SingleAsync(x => x.NeedId == 12);
        Assert.Equal("Новий опис потреби", updated.Description);
        Assert.Equal("Нові реквізити", updated.PaymentDetails);
    }

    [Fact]
    public async Task DeleteNeed()
    {
        await using var db = CreateDbContext();
        db.Shelters.Add(new Shelter
        {
            AccountId = 3,
            ShelterName = "Shelter East",
            Location = "Kharkiv",
            AdminName = "Roman"
        });
        db.ShelterNeeds.Add(new ShelterNeed
        {
            NeedId = 13,
            ShelterId = 3,
            Description = "Потрібні миски",
            PaymentDetails = "Передача в притулок"
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "system_admin", 1);

        var result = await controller.DeleteConfirmed(13);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Потребу видалено.", controller.TempData["Success"]);
        Assert.False(await db.ShelterNeeds.AnyAsync(x => x.NeedId == 13));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static NeedsController CreateController(ApplicationDbContext db, string? role = null, int? userId = null)
    {
        var service = new NeedService(db);
        var normalizedUserId = role != null && !userId.HasValue ? 1 : userId;
        var currentUser = normalizedUserId.HasValue
            ? new ApplicationUser
            {
                Id = normalizedUserId.Value,
                UserName = $"user{normalizedUserId.Value}@petly.test",
                Email = $"user{normalizedUserId.Value}@petly.test"
            }
            : null;
        var userManager = CreateUserManager(currentUser, role);
        var controller = new NeedsController(service, userManager);
        var httpContext = new DefaultHttpContext
        {
            User = CreatePrincipal(currentUser, role)
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
    }

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationUser? currentUser, string? role)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var manager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        manager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(currentUser);
        manager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(role == null
                ? new List<string>()
                : new List<string> { role });

        return manager.Object;
    }

    private static ClaimsPrincipal CreatePrincipal(ApplicationUser? currentUser, string? role)
    {
        if (currentUser == null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, currentUser.Id.ToString()),
            new(ClaimTypes.Name, currentUser.UserName ?? currentUser.Email ?? currentUser.Id.ToString())
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
