using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;

namespace QuickCrew.Data.Entities
{
    public class User : IdentityUser
    {
        private readonly int rating;

        public User()
        {
            //this.Id = Guid.NewGuid().ToString();
            this.Reviews = new HashSet<Review>();
        }

        public string Name { get; set; } = null!;

        public int Rating => Reviews?.Any() == true
            ? (int)Math.Round(Reviews.Average(r => r.Rating))
            : 0;

        [JsonIgnore]
        public IEnumerable<Review> Reviews { get; set; }
    }
}
