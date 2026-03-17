using System;
using System.ComponentModel.DataAnnotations;

namespace TravelGuideWebAdmin.Models
{
    public class Tour
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public TimeSpan EstimatedTime { get; set; }
    }
}
