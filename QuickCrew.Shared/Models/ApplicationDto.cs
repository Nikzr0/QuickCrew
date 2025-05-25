namespace QuickCrew.Shared.Models
{
    public class ApplicationDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }

        public int JobPostingId { get; set; }
        public string UserId { get; set; }

        public string JobTitle { get; set; } 
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }
}