using System.ComponentModel.DataAnnotations;

namespace QuickCrew.Shared.Models
{
    public class JobPostingDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Number of slots needed is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Slots needed must be at least 1.")]
        public int SlotsNeeded { get; set; }

        public int LocationId { get; set; }
        public LocationDto? Location { get; set; }

        public int CategoryId { get; set; }
        public CategoryDto? Category { get; set; }

        public string OwnerId { get; set; } = null!;
        public string OwnerName { get; set; }

        public DateTime CreatedDate { get; set; }

        public double AverageRating { get; set; }
        public ICollection<ReviewDto> Reviews { get; set; } = new HashSet<ReviewDto>();
    }
}