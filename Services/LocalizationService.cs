using Microsoft.Maui.Storage;

namespace TravelGuideApp.Services
{
    public static class LocalizationService
    {
        public static string CurrentLanguage => Preferences.Get("Lang", "vi");

        public static void SetLanguage(string lang)
        {
            if (lang != "vi" && lang != "en")
                lang = "vi";

            Preferences.Set("Lang", lang);
        }
    }
}
