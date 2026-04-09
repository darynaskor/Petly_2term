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

public class AdoptionControllerTests
{
    [Fact]
    public async Task ViewApplicationsWithoutLogin()
    {
        await using var db = CreateDbContext();
        var controller = CreateController(db);

        var result = await controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public async Task ViewUserApplications()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 10, PetName = "Barsik", Status = "Available" });
        db.AdoptionApplications.AddRange(
            new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = AdoptionStatuses.Pending },
            new AdoptionApplication { AdoptId = 12, UserId = 8, PetId = 1, Status = AdoptionStatuses.Pending });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 7);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);

        var application = Assert.Single(model);
        Assert.Equal(7, application.UserId);
        Assert.Equal(1, application.PetId);
    }

    [Fact]
    public async Task ShelterAdminSeesOnlyApplicationsForOwnShelterPets()
    {
        await using var db = CreateDbContext();

        db.Pets.AddRange(
            new Pet { PetId = 1, ShelterId = 5, PetName = "Max", Status = "Available" },
            new Pet { PetId = 2, ShelterId = 9, PetName = "Luna", Status = "Available" });
        db.Users.AddRange(
            new ApplicationUser { Id = 7, Email = "user7@test.com", UserName = "user7@test.com" },
            new ApplicationUser { Id = 8, Email = "user8@test.com", UserName = "user8@test.com" });
        db.AdoptionApplications.AddRange(
            new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = AdoptionStatuses.Pending },
            new AdoptionApplication { AdoptId = 12, UserId = 8, PetId = 2, Status = AdoptionStatuses.Pending });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);

        var application = Assert.Single(model);
        Assert.Equal(1, application.PetId);
    }

    [Fact]
    public async Task SystemAdminSeesAllApplications()
    {
        await using var db = CreateDbContext();

        db.Pets.AddRange(
            new Pet { PetId = 1, ShelterId = 5, PetName = "Max", Status = "Available" },
            new Pet { PetId = 2, ShelterId = 9, PetName = "Luna", Status = "Available" });
        db.Users.AddRange(
            new ApplicationUser { Id = 7, Email = "user7@test.com", UserName = "user7@test.com" },
            new ApplicationUser { Id = 8, Email = "user8@test.com", UserName = "user8@test.com" });
        db.AdoptionApplications.AddRange(
            new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = AdoptionStatuses.Pending },
            new AdoptionApplication { AdoptId = 12, UserId = 8, PetId = 2, Status = AdoptionStatuses.Pending });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "system_admin", 99);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);

        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task OpenAdoptPage()
    {
        await using var db = CreateDbContext();

        db.Users.Add(new ApplicationUser { Id = 9, Name = "Оля", Surname = "Коваль", Email = "user9@test.com", UserName = "user9@test.com" });
        db.Pets.Add(new Pet { PetId = 5, ShelterId = 30, PetName = "Simba", Status = "Available" });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 9);

        var result = await controller.Create(5);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdoptionRequestViewModel>(view.Model);

        Assert.Equal("Simba", model.PetName);
        Assert.Equal("Оля", model.ApplicantName);
    }

    [Fact]
    public async Task AdoptPet()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 6, ShelterId = 41, PetName = "Daisy", Status = "Available" });
        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 13);

        var result = await controller.Adopt(new AdoptionRequestViewModel
        {
            PetId = 6,
            ApplicantName = "Анна",
            ApplicantSurname = "Іваненко",
            ApplicantAge = 24,
            ContactInfo = "+380123456789"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var application = await db.AdoptionApplications.SingleAsync();
        Assert.Equal(13, application.UserId);
        Assert.Equal(AdoptionStatuses.Pending, application.Status);
        Assert.Equal("Анна", application.ApplicantName);
        Assert.Equal("+380123456789", application.ContactInfo);
    }

    [Fact]
    public async Task ViewUserApplications_NormalizesLegacyPendingStatus()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 5, PetName = "Max", Status = "Доступний" });
        db.AdoptionApplications.Add(new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = "Pending" });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "user", 7);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<AdoptionApplication>>(view.Model);

        var application = Assert.Single(model);
        Assert.Equal(11, application.AdoptId);
        Assert.Equal(AdoptionStatuses.Pending, application.Status);
    }

    [Fact]
    public async Task ShelterAdminCanApproveOwnShelterApplication()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 5, PetName = "Max", Status = "Доступний" });
        db.AdoptionApplications.AddRange(
            new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = AdoptionStatuses.Pending },
            new AdoptionApplication { AdoptId = 12, UserId = 8, PetId = 1, Status = "Pending" });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Approve(11);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var approved = await db.AdoptionApplications.SingleAsync(a => a.AdoptId == 11);
        var autoRejected = await db.AdoptionApplications.SingleAsync(a => a.AdoptId == 12);
        var pet = await db.Pets.SingleAsync();

        Assert.Equal(AdoptionStatuses.Approved, approved.Status);
        Assert.Equal(AdoptionStatuses.AutoRejected, autoRejected.Status);
        Assert.Equal("Прилаштований", pet.Status);
    }

    [Fact]
    public async Task ShelterAdminCannotApproveAnotherShelterApplication()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 55, PetName = "Max", Status = "Доступний" });
        db.AdoptionApplications.Add(new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = AdoptionStatuses.Pending });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Approve(11);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ShelterAdminCanRejectApplication()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 5, PetName = "Max", Status = "Доступний" });
        db.AdoptionApplications.Add(new AdoptionApplication { AdoptId = 11, UserId = 7, PetId = 1, Status = AdoptionStatuses.Pending });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.Reject(11);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var application = await db.AdoptionApplications.SingleAsync();
        Assert.Equal(AdoptionStatuses.Rejected, application.Status);
    }

    [Fact]
    public async Task ShelterAdminCanViewApplicantDetailsForOwnShelterApplication()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 5, PetName = "Max", Status = "Доступний" });
        db.Users.Add(new ApplicationUser { Id = 7, Name = "Іра", Surname = "Коваль", Email = "ira@test.com", UserName = "ira@test.com" });
        db.AdoptionApplications.Add(new AdoptionApplication
        {
            AdoptId = 11,
            UserId = 7,
            PetId = 1,
            Status = AdoptionStatuses.Pending,
            ApplicantName = "Іра",
            ApplicantSurname = "Коваль",
            ApplicantAge = 22,
            ContactInfo = "@ira"
        });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.UserDetails(11);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdoptionApplication>(view.Model);

        Assert.Equal(7, model.UserId);
        Assert.Equal("@ira", model.ContactInfo);
    }

    [Fact]
    public async Task ShelterAdminCannotViewApplicantDetailsForAnotherShelterApplication()
    {
        await using var db = CreateDbContext();

        db.Pets.Add(new Pet { PetId = 1, ShelterId = 55, PetName = "Max", Status = "Доступний" });
        db.Users.Add(new ApplicationUser { Id = 7, Name = "Іра", Surname = "Коваль", Email = "ira@test.com", UserName = "ira@test.com" });
        db.AdoptionApplications.Add(new AdoptionApplication
        {
            AdoptId = 11,
            UserId = 7,
            PetId = 1,
            Status = AdoptionStatuses.Pending,
            ApplicantName = "Іра",
            ApplicantSurname = "Коваль",
            ApplicantAge = 22,
            ContactInfo = "@ira"
        });

        await db.SaveChangesAsync();

        var controller = CreateController(db, "shelter_admin", 5);

        var result = await controller.UserDetails(11);

        Assert.IsType<ForbidResult>(result);
    }

    // ================= HELPERS =================

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AdoptionController CreateController(ApplicationDbContext db, string? role = null, int? accountId = null)
    {
        var adoptionService = new AdoptionService(db);
        var petService = new PetService(db);
        var normalizedAccountId = role != null && !accountId.HasValue ? 1 : accountId;
        var existingUser = normalizedAccountId.HasValue
            ? db.Users.FirstOrDefault(u => u.Id == normalizedAccountId.Value)
            : null;
        var currentUser = normalizedAccountId.HasValue
            ? new ApplicationUser
            {
                Id = normalizedAccountId.Value,
                UserName = existingUser?.UserName ?? $"user{normalizedAccountId.Value}@petly.test",
                Email = existingUser?.Email ?? $"user{normalizedAccountId.Value}@petly.test",
                Name = existingUser?.Name,
                Surname = existingUser?.Surname
            }
            : null;
        var userManager = GetUserManager(currentUser, role);

        var controller = new AdoptionController(adoptionService, petService, userManager);

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

    private static UserManager<ApplicationUser> GetUserManager(ApplicationUser? currentUser, string? role)
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
        manager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns((ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.NameIdentifier));
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
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
