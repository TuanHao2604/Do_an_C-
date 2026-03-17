using System.ComponentModel.DataAnnotations;

namespace TravelGuideWebAdmin.Models
{
    public class POI_Image
    {
        [Key]
        public int Id { get; set; }
        public int PoiId { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
    }
}
