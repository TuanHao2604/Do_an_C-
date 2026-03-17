using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Threading;
using TravelGuideApp.Models;
using TravelGuideApp.Database;
using TravelGuideApp.Services;
using TravelGuideApp.Views;

namespace TravelGuideApp.ViewModels
{
    public partial class MapViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;
        private readonly LocationService _locationService;
        private readonly GeofenceService _geofenceService;
        private readonly NarrationService _narrationService;
        private readonly SyncService _syncService;
        private CancellationTokenSource _trackingCts;

        public ObservableCollection<POI> PointsOfInterest { get; } = new();

        [ObservableProperty]
        POI selectedPoi;

        public MapViewModel(SQLiteService database, LocationService locationService, GeofenceService geofenceService, NarrationService narrationService, SyncService syncService)
        {
            Title = "Map";
            _database = database;
            _locationService = locationService;
            _geofenceService = geofenceService;
            _narrationService = narrationService;
            _syncService = syncService;
        }

        partial void OnSelectedPoiChanged(POI value)
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

        [RelayCommand]
        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                await _syncService.TrySyncPoisAsync();
                PointsOfInterest.Clear();
                var pois = await _database.GetPOIsAsync();
                foreach (var poi in pois)
                {
                    PointsOfInterest.Add(poi);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        [RelayCommand]
        async Task CheckGeofenceAsync()
        {
            var location = await _locationService.GetCurrentLocationAsync();
            if (location != null && PointsOfInterest.Count > 0)
            {
                var triggeredPoi = _geofenceService.CheckGeofence(location, PointsOfInterest.ToList());
                if (triggeredPoi != null)
                {
                    await _narrationService.SpeakAsync($"Bạn đang đến {triggeredPoi.Name}. {triggeredPoi.Description}");
                    await NotificationService.ShowAsync("POI nổi tiếng gần bạn", $"{triggeredPoi.Name} - chạm để khám phá.");
                    var username = Preferences.Get("Username", string.Empty);
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var user = await _database.GetUserByUsernameAsync(username);
                        if (user != null)
                        {
                            await _database.AddUserPoiLogAsync(user.Id, triggeredPoi.Id, "gps");
                            await _syncService.TryPostUserPoiLogAsync(username, triggeredPoi.Id, "gps", DateTime.UtcNow);
                        }
                    }
                }
            }
        }

        [RelayCommand]
        async Task StartTrackingAsync()
        {
            if (_trackingCts != null)
                return;

            _trackingCts = new CancellationTokenSource();
            await _locationService.ListenForLocationUpdatesAsync(async location =>
            {
                if (location == null || PointsOfInterest.Count == 0)
                    return;

                var triggeredPoi = _geofenceService.CheckGeofence(location, PointsOfInterest.ToList());
                if (triggeredPoi != null)
                {
                    await _narrationService.SpeakAsync($"Bạn đang đến {triggeredPoi.Name}. {triggeredPoi.Description}");
                    await NotificationService.ShowAsync("POI nổi tiếng gần bạn", $"{triggeredPoi.Name} - chạm để khám phá.");
                    var username = Preferences.Get("Username", string.Empty);
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var user = await _database.GetUserByUsernameAsync(username);
                        if (user != null)
                        {
                            await _database.AddUserPoiLogAsync(user.Id, triggeredPoi.Id, "gps");
                            await _syncService.TryPostUserPoiLogAsync(username, triggeredPoi.Id, "gps", DateTime.UtcNow);
                        }
                    }
                }
            }, TimeSpan.FromSeconds(15), _trackingCts.Token);
        }

        [RelayCommand]
        void StopTracking()
        {
            if (_trackingCts == null)
                return;

            _trackingCts.Cancel();
            _trackingCts = null;
        }
    }
}
