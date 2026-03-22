using System.ComponentModel.DataAnnotations;

namespace Petly.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введіть email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ім'я")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть прізвище")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    [MinLength(4, ErrorMessage = "Мінімум 4 символи")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    // Додано для вибору ролі
    public string Role { get; set; } = "user"; // default = user
}

public class UserEditViewModel
{
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Введіть ім'я")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть прізвище")]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "user";

    [Required]
    public string Status { get; set; } = "Активний";

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}