using SQLite;

namespace TravelGuideApp.Models
{
    public class User_Favorite_Tour
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int UserId { get; set; }
        [Indexed]
        public int TourId { get; set; }
    }
}
