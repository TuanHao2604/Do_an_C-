using SQLite;

namespace TravelGuideApp.Models
{
    public class Tour_POI
    {
        [Indexed]
        public int TourId { get; set; }
        [Indexed]
        public int PoiId { get; set; }
        public int OrderIndex { get; set; }
    }
}
