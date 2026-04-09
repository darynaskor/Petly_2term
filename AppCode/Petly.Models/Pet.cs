using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petly.Models;

[Table("pet")]
public class Pet
{
    [Key]
    [Column("petId")]
    public int PetId { get; set; }

    [Column("shelterId")]
    public int ShelterId { get; set; }

    [Required]
    [Column("petName")]
    public string PetName { get; set; } = string.Empty;

    [Column("type")]
    public string? Type { get; set; }

    [Column("breed")]
    public string? Breed { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("age")]
    public int? Age { get; set; }
    
    [NotMapped]
    public string AgeText => Age switch
    {
        1 => "рік",
        2 or 3 or 4 => "роки",
        _ => "років"
    };

    [Column("size")]
    public string? Size { get; set; }

    [Column("vaccinated")]
    public bool Vaccinated { get; set; }

    [Column("sterilized")]
    public bool Sterilized { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Available";

    [Column("photoUrl")]
    public string? PhotoUrl { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}