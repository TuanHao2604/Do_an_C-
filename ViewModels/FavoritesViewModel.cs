using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Models;
using Microsoft.Maui.ApplicationModel;
using TravelGuideApp.Views;

namespace TravelGuideApp.ViewModels
{
    public class FavoritesViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;

        public ObservableCollection<POI> FavoritePois { get; } = new();
        public ObservableCollection<Tour> FavoriteTours { get; } = new();

        private POI _selectedPoi;
        public POI SelectedPoi
        {
            get => _selectedPoi;
            set
            {
                if (SetProperty(ref _selectedPoi, value))
                    OnSelectedPoiChanged(value);
            }
        }

        private Tour _selectedTour;
        public Tour SelectedTour
        {
            get => _selectedTour;
            set
            {
                if (SetProperty(ref _selectedTour, value))
                    OnSelectedTourChanged(value);
            }
        }

        public FavoritesViewModel(SQLiteService database)
        {
            _database = database;
            Title = "Yêu thích";
        }

        public async Task LoadAsync()
        {
            var username = Preferences.Get("Username", string.Empty);
            if (string.IsNullOrWhiteSpace(username))
                return;

            var user = await _database.GetUserByUsernameAsync(username);
            if (user == null)
                return;

            FavoritePois.Clear();
            FavoriteTours.Clear();

            var pois = await _database.GetFavoritePoisAsync(user.Id);
            foreach (var poi in pois)
                FavoritePois.Add(poi);

            var tours = await _database.GetFavoriteToursAsync(user.Id);
            foreach (var tour in tours)
                FavoriteTours.Add(tour);
        }

        private void OnSelectedPoiChanged(POI value)
        {
            if (value == null)
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync(nameof(POIDetailPage), new Dictionary<string, object>
                {
                    { "poiId", value.Id }
                });
            });

            SelectedPoi = null;
        }

        private void OnSelectedTourChanged(Tour value)
        {
            if (value == null)
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync(nameof(TourDetailPage), new Dictionary<string, object>
                {
                    { "tourId", value.Id }
                });
            });

            SelectedTour = null;
        }
    }
}
