using System.ComponentModel.DataAnnotations;

namespace QuickCrew.Shared.Models
{
    public class ReviewDto
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // Рейтинг (1-5)

        [MaxLength(1000)]
        public string Comment { get; set; } // Коментар

        public DateTime ReviewedAt { get; set; } // Дата на ревюто

        // Връзки към други обекти (ID-та)
        public string ReviewerId { get; set; } // ID на рецензента
        public int JobPostingId { get; set; } // ID на обявата

        // Допълнителна информация (не се записва в базата)
        public string ReviewerName { get; set; } // Име на рецензента
        public string JobTitle { get; set; } // Заглавие на обявата
    }
}