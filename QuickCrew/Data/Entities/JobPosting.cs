using System.Text.Json.Serialization;

using QuickCrew.Data.Common.Models;

namespace QuickCrew.Data.Entities
{
    public class JobPosting : BaseModel<int>
    {
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int SlotsNeeded { get; set; }

        public int LocationId { get; set; }

        [JsonIgnore]
        public Location? Location { get; set; }

        public int CategoryId { get; set; }

        [JsonIgnore]
        public Category? Category { get; set; }

        public string OwnerId { get; set; } = null!;

        [JsonIgnore]
        public User? Owner { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
