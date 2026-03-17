using SQLite;
using System;

namespace TravelGuideApp.Models
{
    public class Review
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int UserId { get; set; }
        [Indexed]
        public int TourId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
