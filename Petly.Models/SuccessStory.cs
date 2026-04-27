using System;
using System.ComponentModel.DataAnnotations;

namespace Petly.Models
{
    public class SuccessStory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть ім'я тваринки")]
        [Display(Name = "Ім'я пухнастика")]
        public string PetName { get; set; }

        [Required(ErrorMessage = "Додайте заголовок історії")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Напишіть текст історії")]
        [Display(Name = "Історія")]
        public string StoryText { get; set; }

        [Display(Name = "Посилання на фото")]
        public string? ImageUrl { get; set; } // Зробимо поки що необов'язковим

        [Display(Name = "Дата публікації")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}