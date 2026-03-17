namespace TravelGuideApp;

public partial class AppShell : Shell
{
        public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Views.POIDetailPage), typeof(Views.POIDetailPage));
        Routing.RegisterRoute(nameof(Views.AudioGuidePage), typeof(Views.AudioGuidePage));
        Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
        Routing.RegisterRoute(nameof(Views.TourDetailPage), typeof(Views.TourDetailPage));
        Routing.RegisterRoute(nameof(Views.ProfilePage), typeof(Views.ProfilePage));
        Routing.RegisterRoute(nameof(Views.HistoryPage), typeof(Views.HistoryPage));
        Routing.RegisterRoute(nameof(Views.FavoritesPage), typeof(Views.FavoritesPage));
        Routing.RegisterRoute(nameof(Views.TourReviewPage), typeof(Views.TourReviewPage));
    }
}
