using QuickCrew.Shared.Models;

namespace QuickCrew.Web.Models
{
    public class DashboardViewModel
    {
        public int ActiveJobs { get; set; }
        public int RegisteredUsers { get; set; }
        public int ActiveProjects { get; set; }
        public List<JobPostingDto> RecentJobPostings { get; set; } = new List<JobPostingDto>();
    }
    public class PlatformStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveProjects { get; set; }
    }
}