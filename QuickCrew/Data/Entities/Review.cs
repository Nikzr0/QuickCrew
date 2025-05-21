using QuickCrew.Data.Common.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuickCrew.Data.Entities
{
    public class Review : BaseModel<int>
    {
        private int rating;

        public string ReviewerId { get; set; }

        [ForeignKey(nameof(ReviewerId))]
        [JsonIgnore]
        public User Reviewer { get; set; }

        public int JobPostingId { get; set; }

        [ForeignKey(nameof(JobPostingId))]
        [JsonIgnore]
        public JobPosting JobPosting { get; set; }

        public int Rating
        {
            get => rating;
            set => rating = value is >= 1 and <= 5
                ? value
                : throw new InvalidOperationException("Rating must be 1-5");
        }

        public string Comment { get; set; }
        public DateTime ReviewedAt { get; internal set; } = DateTime.UtcNow;
    }
}