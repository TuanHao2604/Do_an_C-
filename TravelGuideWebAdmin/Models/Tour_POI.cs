using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelGuideWebAdmin.Models
{
    public class Tour_POI
    {
        [Key]
        public int Id { get; set; }

        public int TourId { get; set; }
        public int PoiId { get; set; }
        public int OrderIndex { get; set; }
        
        [ForeignKey("TourId")]
        public Tour Tour { get; set; }
        
        [ForeignKey("PoiId")]
        public POI POI { get; set; }
    }
}
