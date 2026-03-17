using SQLite;
using System;

namespace TravelGuideApp.Models
{
    public class User_POI_Log
    {
        [Indexed]
        public int UserId { get; set; }
        [Indexed]
        public int PoiId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TriggerType { get; set; } // "gps" or "qr"
    }
}
