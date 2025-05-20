namespace QuickCrew.Shared.Models
{
    public class JobPostingDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int SlotsNeeded { get; set; }
        public int LocationId { get; set; }
        public int CategoryId { get; set; }
    }
}