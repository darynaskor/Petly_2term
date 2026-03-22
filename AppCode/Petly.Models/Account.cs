using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petly.Models;

[Table("account")]
public class Account
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("registrationDate")]
    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    [Required]
    [Column("role")]
    public string Role { get; set; } = "user";

    public UserProfile? UserProfile { get; set; }
}
