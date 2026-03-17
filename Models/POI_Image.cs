using SQLite;

namespace TravelGuideApp.Models
{
    public class POI_Image
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PoiId { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
    }
}
