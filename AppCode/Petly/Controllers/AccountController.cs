using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Petly.Business.Services;
using Petly.Models;

namespace Petly.Controllers;

public class AccountController : Controller
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var account = await _accountService.GetByCredentialsAsync(model.Email, model.Password);
        if (account == null)
        {
            ModelState.AddModelError(string.Empty, "Неправильний email або пароль");
            return View(model);
        }

        SetSession(account);
        TempData["Success"] = $"Привіт, {account.UserProfile?.Name ?? "користувачу"}!";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _accountService.EmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Такий email вже існує");
            return View(model);
        }

        var profile = await _accountService.CreateUserAsync(model);
        SetSession(profile.Account!);
        TempData["Success"] = "Користувача успішно зареєстровано";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        if (!IsAdmin()) return Forbid();
        var users = await _accountService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAdmin()) return Forbid();
        var profile = await _accountService.GetUserAsync(id);
        if (profile == null)
            return NotFound();

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!IsAdmin()) return Forbid();
        if (!ModelState.IsValid)
            return View(model);

        if (await _accountService.EmailExistsAsync(model.Email, model.AccountId))
        {
            ModelState.AddModelError("Email", "Такий email вже використовується");
            return View(model);
        }

        await _accountService.UpdateUserAsync(model);
        TempData["Success"] = "Дані користувача оновлено";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin()) return Forbid();
        await _accountService.DeleteUserAsync(id);
        TempData["Success"] = "Користувача видалено";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    private void SetSession(Account account)
{
    HttpContext.Session.SetInt32("AccountId", account.Id);
    HttpContext.Session.SetString("UserName", account.UserProfile?.Name ?? account.Email);
    HttpContext.Session.SetString("Role", account.Role);

    
    HttpContext.Session.SetString("UserEmail", account.Email);
}

    private bool IsAdmin() => HttpContext.Session.GetString("Role") == "system_admin";

    private bool IsShelterAdmin() => HttpContext.Session.GetString("Role") == "shelter_admin";
}
