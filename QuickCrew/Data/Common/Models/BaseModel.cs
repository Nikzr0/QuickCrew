using System.ComponentModel.DataAnnotations;

namespace QuickCrew.Data.Common.Models
{
    public class BaseModel<TKey>
    {
        [Key]
        public TKey Id { get; set; } = default!;
    }
}
