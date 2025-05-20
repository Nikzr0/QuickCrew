using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Identity;

namespace QuickCrew.Data.Entities
{
    public class User : IdentityUser
    {
        private readonly int rating;

        public User()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Reviews = new HashSet<Review>();
        }

        public string Name { get; set; } = null!;

        public int Rating
        {
            get
            {
                if (!this.Reviews.Any())
                {
                    return 0;
                }

                return (int)Math.Round(this.Reviews.Average(r => r.Rating));
            }
        }

        [JsonIgnore]
        public IEnumerable<Review> Reviews { get; set; }
    }
}
