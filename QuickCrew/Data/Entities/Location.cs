using QuickCrew.Data.Common.Models;

namespace QuickCrew.Data.Entities
{
    public class Location : BaseModel<int>
    {
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string ZipCode { get; set; } = null!;

        public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    }
}
