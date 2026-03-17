namespace TravelGuideApp.Services
{
    public static class ApiSettings
    {
        // For Android emulator use 10.0.2.2 to access host machine.
        // For real device, change to your machine's LAN IP.
        public const string BaseUrl = "http://10.0.2.2:5014";
        public const string ClientId = "travelguide-app";
        public const string ClientSecret = "change-me";
    }
}
