using QuickCrew.Shared.Models;
using System.Collections.Generic;

namespace QuickCrew.Web.Models
{
    public class DashboardViewModel
    {
        public List<JobPostingDto> RecentJobPostings { get; set; } = new List<JobPostingDto>();

        public int ActiveJobs { get; set; }
        public int RegisteredUsers { get; set; }
        public int ActiveProjects { get; set; }
    }
}