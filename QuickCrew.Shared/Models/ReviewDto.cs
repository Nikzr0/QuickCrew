using System.ComponentModel.DataAnnotations;

namespace QuickCrew.Shared.Models
{
    public class ReviewDto
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        public DateTime ReviewedAt { get; set; }

        public string ReviewerId { get; set; }
        public int JobPostingId { get; set; }

        public string ReviewerName { get; set; }
        public string JobTitle { get; set; }
    }
}