using System;
using System.ComponentModel.DataAnnotations;

namespace TravelGuideWebAdmin.Models
{
    public class User_POI_Log
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public int PoiId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TriggerType { get; set; } // "gps" or "qr"
    }
}
