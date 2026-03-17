using System.ComponentModel.DataAnnotations;

namespace TravelGuideWebAdmin.Models
{
    public class POI
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public int Priority { get; set; }
    }
}
