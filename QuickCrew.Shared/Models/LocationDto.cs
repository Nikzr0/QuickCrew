using System.ComponentModel.DataAnnotations;

namespace QuickCrew.Shared.Models
{
    public class LocationDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(50)]
        public string State { get; set; }

        [Required]
        [MaxLength(20)]
        public string ZipCode { get; set; }

        public string FullAddress => $"{Address}, {City}, {State} {ZipCode}";
    }
}