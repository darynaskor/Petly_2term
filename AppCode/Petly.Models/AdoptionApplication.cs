using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petly.Models;

[Table("adoptionapplication")]
public class AdoptionApplication
{
    [Key]
    [Column("adoptId")]
    public int AdoptId { get; set; }

    [Column("userId")]
    public int UserId { get; set; }

    [Column("petId")]
    public int PetId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Очікує";

    [Column("submissionDate")]
    public DateTime SubmissionDate { get; set; } = DateTime.Now;

   
    [ForeignKey("PetId")]
    public Pet? Pet { get; set; }
}