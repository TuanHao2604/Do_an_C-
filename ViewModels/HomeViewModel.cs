using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using TravelGuideApp.Models;
using TravelGuideApp.Database;
using TravelGuideApp.Services;
using TravelGuideApp.Views;

namespace TravelGuideApp.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;
        private readonly SyncService _syncService;

        public ObservableCollection<POI> TopLocations { get; } = new();
        public ObservableCollection<POI> FilteredLocations { get; } = new();

        [ObservableProperty]
        POI selectedPoi;

        [ObservableProperty]
        string searchQuery;

        public HomeViewModel(SQLiteService database, SyncService syncService)
        {
            Title = "Khám phá";
            _database = database;
            _syncService = syncService;
        }

        [RelayCommand]
        public async Task LoadHomeDataAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                await _syncService.TrySyncPoisAsync();
                await _syncService.TrySyncPoiAssetsAsync();
                TopLocations.Clear();
                FilteredLocations.Clear();
                var pois = await _database.GetPOIsAsync();
                foreach (var poi in pois.Take(3)) // Get top 3
                {
                    TopLocations.Add(poi);
                }
                foreach (var poi in pois)
                {
                    FilteredLocations.Add(poi);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SyncAsync()
        {
            await LoadHomeDataAsync();
        }

        partial void OnSearchQueryChanged(string value)
        {
            ApplyFilter(value);
        }

        private async void ApplyFilter(string value)
        {
            var query = value?.Trim() ?? string.Empty;
            var pois = await _database.GetPOIsAsync();

            FilteredLocations.Clear();
            foreach (var poi in pois)
            {
                if (string.IsNullOrWhiteSpace(query) ||
                    poi.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(poi.Description) && poi.Description.Contains(query, StringComparison.OrdinalIgnoreCase)))
                {
                    FilteredLocations.Add(poi);
                }
            }
        }

        partial void OnSelectedPoiChanged(POI value)
        {
            if (value == null)
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Views.POIDetailPage), new Dictionary<string, object>
                {
                    { "poiId", value.Id }
                });
            });

            SelectedPoi = null;
        }
    }
}
