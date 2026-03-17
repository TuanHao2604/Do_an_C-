using SQLite;

namespace TravelGuideApp.Models
{
    public class User_Favorite_POI
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int UserId { get; set; }
        [Indexed]
        public int PoiId { get; set; }
    }
}
