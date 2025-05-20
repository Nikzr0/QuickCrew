using QuickCrew.Data.Common.Models;

namespace QuickCrew.Data.Entities
{
    public class Category : BaseModel<int>
    {
        public string Name { get; set; } = null!;
    }
}
