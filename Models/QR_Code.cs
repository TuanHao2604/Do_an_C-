using SQLite;

namespace TravelGuideApp.Models
{
    public class QR_Code
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PoiId { get; set; }
        public string QrValue { get; set; }
    }
}
