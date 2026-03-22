using System.ComponentModel.DataAnnotations;

namespace Petly.Models;

public class ShelterNeedCardViewModel
{
    public int NeedId { get; set; }

    public int ShelterId { get; set; }

    public string ShelterName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string PaymentDetails { get; set; } = string.Empty;

    public bool CanManage { get; set; }
}

public class ShelterNeedFormViewModel
{
    public int NeedId { get; set; }

    [Required(ErrorMessage = "Опиши, що саме потрібно")]
    [Display(Name = "Що потрібно")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Додай спосіб допомоги або оплати")]
    [Display(Name = "Як допомогти")]
    [StringLength(255, ErrorMessage = "До 255 символів")]
    public string PaymentDetails { get; set; } = string.Empty;
}
