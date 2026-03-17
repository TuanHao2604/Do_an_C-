using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Services;
using TravelGuideApp.Views;

namespace TravelGuideApp.ViewModels
{
    public partial class QRScanViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;
        private readonly NarrationService _narrationService;
        private readonly SyncService _syncService;
        private bool _isHandlingScan;

        public QRScanViewModel(SQLiteService database, NarrationService narrationService, SyncService syncService)
        {
            Title = "QR Scanner";
            _database = database;
            _narrationService = narrationService;
            _syncService = syncService;
        }

        [RelayCommand]
        public async Task SimulateScanAsync()
        {
            var pois = await _database.GetPOIsAsync();
            if (pois.Count == 0)
            {
                await _syncService.TrySyncPoisAsync();
                pois = await _database.GetPOIsAsync();
            }
            var firstPoi = pois.FirstOrDefault();
            if (firstPoi != null)
            {
                await Shell.Current.GoToAsync(nameof(POIDetailPage), new Dictionary<string, object>
                {
                    { "poiId", firstPoi.Id }
                });
                await _narrationService.SpeakAsync($"Mô phỏng quét QR thành công: {firstPoi.Name}. {firstPoi.Description}");
                return;
            }

            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Thông báo", "Chưa có dữ liệu POI để mô phỏng quét.", "OK");
            }
        }

        public async Task HandleScannedValueAsync(string value)
        {
            if (_isHandlingScan || string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                _isHandlingScan = true;

                var poi = await _database.GetPoiByQrValueAsync(value);
                if (poi == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Thông báo", "QR không hợp lệ hoặc chưa được gán POI.", "OK");
                    return;
                }

                await Shell.Current.GoToAsync(nameof(POIDetailPage), new Dictionary<string, object>
                {
                    { "poiId", poi.Id }
                });
                await _narrationService.SpeakAsync($"Bạn đang đến {poi.Name}. {poi.Description}");
                await NotificationService.ShowAsync("Quét QR thành công", poi.Name);

                var username = Preferences.Get("Username", string.Empty);
                if (!string.IsNullOrWhiteSpace(username))
                {
                    var user = await _database.GetUserByUsernameAsync(username);
                    if (user != null)
                    {
                        await _database.AddUserPoiLogAsync(user.Id, poi.Id, "qr");
                        await _syncService.TryPostUserPoiLogAsync(username, poi.Id, "qr", DateTime.UtcNow);
                    }
                }
            }
            finally
            {
                _isHandlingScan = false;
            }
        }
    }
}
