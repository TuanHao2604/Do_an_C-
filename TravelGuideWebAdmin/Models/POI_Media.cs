using System.ComponentModel.DataAnnotations;

namespace TravelGuideWebAdmin.Models
{
    public class POI_Media
    {
        [Key]
        public int Id { get; set; }
        public int PoiId { get; set; }
        public string Type { get; set; } // "audio" or "tts"
        public string AudioUrl { get; set; }
        public string TtsScript { get; set; }
        public string Language { get; set; }
    }
}
