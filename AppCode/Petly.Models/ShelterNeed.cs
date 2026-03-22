using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petly.Models;

[Table("shelterneed")]
public class ShelterNeed
{
    [Key]
    [Column("needId")]
    public int NeedId { get; set; }

    [Column("shelterId")]
    public int ShelterId { get; set; }

    [Required(ErrorMessage = "Опиши, що саме потрібно")]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Додай спосіб допомоги або оплати")]
    [Column("paymentDetails")]
    [StringLength(255)]
    public string PaymentDetails { get; set; } = string.Empty;

    [ForeignKey(nameof(ShelterId))]
    public Shelter? Shelter { get; set; }
}
