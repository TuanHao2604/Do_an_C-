using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Models;

namespace TravelGuideApp.ViewModels
{
    public partial class TourDetailViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;

        [ObservableProperty]
        Tour tour;

        public ObservableCollection<TourPoiItem> TourPois { get; } = new();

        [ObservableProperty]
        bool isFavorite;

        [ObservableProperty]
        string localizedDescription;

        public TourDetailViewModel(SQLiteService database)
        {
            _database = database;
            Title = "Chi tiết tour";
        }

        public async Task LoadByIdAsync(int tourId)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Tour = await _database.GetTourByIdAsync(tourId);
                Title = Tour?.Name;
                LocalizedDescription = GetLocalizedDescription(Tour);

                TourPois.Clear();
                var pois = await _database.GetPOIsForTourAsync(tourId);
                var orderIndex = 1;
                foreach (var poi in pois)
                {
                    TourPois.Add(new TourPoiItem
                    {
                        OrderIndex = orderIndex++,
                        Poi = poi
                    });
                }

                IsFavorite = await GetIsFavoriteAsync(tourId);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetLocalizedDescription(Tour tour)
        {
            if (tour == null)
                return string.Empty;

            var lang = Preferences.Get("Lang", "vi");
            if (lang == "en" && !string.IsNullOrWhiteSpace(tour.DescriptionEn))
                return tour.DescriptionEn;
            return tour.Description;
        }

        private async Task<bool> GetIsFavoriteAsync(int tourId)
        {
            var username = Preferences.Get("Username", string.Empty);
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var user = await _database.GetUserByUsernameAsync(username);
            if (user == null)
                return false;

            return await _database.IsTourFavoriteAsync(user.Id, tourId);
        }

        [RelayCommand]
        async Task ToggleFavoriteAsync()
        {
            if (Tour == null)
                return;

            var username = Preferences.Get("Username", string.Empty);
            if (string.IsNullOrWhiteSpace(username))
            {
                await Shell.Current.DisplayAlert("Thông báo", "Vui lòng đăng nhập để sử dụng yêu thích.", "OK");
                return;
            }

            var user = await _database.GetUserByUsernameAsync(username);
            if (user == null)
                return;

            await _database.ToggleTourFavoriteAsync(user.Id, Tour.Id);
            IsFavorite = await _database.IsTourFavoriteAsync(user.Id, Tour.Id);
        }

        [RelayCommand]
        async Task StartTourAsync()
        {
            if (Tour == null)
                return;

            await Shell.Current.DisplayAlert("Bắt đầu tour", $"Đã bắt đầu: {Tour.Name}", "OK");
            await Shell.Current.GoToAsync("//MapPage");

            var username = Preferences.Get("Username", string.Empty);
            if (!string.IsNullOrWhiteSpace(username))
            {
                var user = await _database.GetUserByUsernameAsync(username);
                if (user != null)
                {
                    user.Points += 50;
                    if (user.Points >= 100)
                        user.Tier = "VIP";
                    await _database.UpdateUserAsync(user);
                }
            }
        }

        [RelayCommand]
        async Task ReviewAsync()
        {
            if (Tour == null)
                return;

            await Shell.Current.GoToAsync(nameof(Views.TourReviewPage), new Dictionary<string, object>
            {
                { "tourId", Tour.Id }
            });
        }
    }

    public class TourPoiItem
    {
        public int OrderIndex { get; set; }
        public POI Poi { get; set; }
    }
}
