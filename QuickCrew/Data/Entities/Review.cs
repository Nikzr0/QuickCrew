using System.Text.Json.Serialization;

using QuickCrew.Data.Common.Models;

namespace QuickCrew.Data.Entities
{
    public class Review : BaseModel<int>
    {
        private int rating;

        public string ReviewerId { get; set; } = null!;

        [JsonIgnore]
        public User? Reviewer { get; set; } = null!;

        public string JobPostingId { get; set; } = null!;

        [JsonIgnore]
        public JobPosting? JobPosting { get; set; } = null!;

        public int Rating
        {
            get => this.rating;
            set
            {
                if (1 <= value && value <= 5)
                {
                    this.rating = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public string Comment { get; set; } = null!;

        public DateTime ReviewedAt { get; private set; } = DateTime.UtcNow;
    }
}
