using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System;

namespace QuickCrew.Data.Entities
{
    public class User : IdentityUser
    {
        private readonly int rating;

        public User()
        {
            this.Reviews = new HashSet<Review>();
            this.JobPostings = new HashSet<JobPosting>();
        }

        public string Name { get; set; } = null!;

        public int Rating => Reviews?.Any() == true
            ? (int)Math.Round(Reviews.Average(r => r.Rating))
            : 0;

        [JsonIgnore]
        public IEnumerable<Review> Reviews { get; set; }

        [JsonIgnore]
        public ICollection<JobPosting> JobPostings { get; set; }
    }
}