using System.Text.Json.Serialization;

using QuickCrew.Data.Common.Models;

namespace QuickCrew.Data.Entities
{
    public class Application : BaseModel<int>
    {
        public string Status { get; set; } = "Pending";

        public DateTime AppliedAt { get; internal set; } = DateTime.UtcNow;

        public int JobPostingId { get; set; }

        [JsonIgnore]
        public JobPosting? JobPosting { get; set; } = null!;

        public string UserId { get; set; } = null!;

        [JsonIgnore]
        public User? User { get; set; } = null!;
    }
}
