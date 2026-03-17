using SQLite;

namespace TravelGuideApp.Models
{
    public class POI_Media
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PoiId { get; set; }
        public string Type { get; set; } // "audio" or "tts"
        public string AudioUrl { get; set; }
        public string TtsScript { get; set; }
        public string Language { get; set; }
    }
}
