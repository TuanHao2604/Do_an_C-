using SQLite;
using System;

namespace TravelGuideApp.Models
{
    public class Tour
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public TimeSpan EstimatedTime { get; set; }
    }
}
