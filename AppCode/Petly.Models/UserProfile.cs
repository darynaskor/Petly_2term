using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petly.Models;

[Table("user")]
public class UserProfile
{
    [Key]
    [ForeignKey(nameof(Account))]
    [Column("accountId")]
    public int AccountId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("surname")]
    public string? Surname { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    public Account? Account { get; set; }
}
