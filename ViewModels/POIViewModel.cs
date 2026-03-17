using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Models;
using TravelGuideApp.Services;

namespace TravelGuideApp.ViewModels
{
    public partial class POIViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;
        private readonly NarrationService _narrationService;
        private readonly OfflineContentService _offlineContentService;

        [ObservableProperty]
        POI item;

        public ObservableCollection<POI_Image> Images { get; } = new();

        [ObservableProperty]
        bool hasImages;

        [ObservableProperty]
        bool isFavorite;

        [ObservableProperty]
        string localizedDescription;

        [ObservableProperty]
        bool isDownloading;

        [ObservableProperty]
        string audioSource;

        [ObservableProperty]
        bool hasAudio;

        private List<POI_Media> _media = new();

        public POIViewModel(SQLiteService database, NarrationService narrationService, OfflineContentService offlineContentService)
        {
            _database = database;
            _narrationService = narrationService;
            _offlineContentService = offlineContentService;
        }

        public async Task LoadByIdAsync(int poiId)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                Item = await _database.GetPOIByIdAsync(poiId);
                Title = Item?.Name;
                LocalizedDescription = GetLocalizedDescription(Item);

                Images.Clear();
                if (Item != null)
                {
                    var images = await _database.GetPoiImagesAsync(Item.Id);
                    foreach (var image in images)
                    {
                        if (_offlineContentService.TryGetCachedPath(image.ImageUrl, $"poi_{Item.Id}/images", out var localPath))
                        {
                            image.ImageUrl = localPath;
                        }
                        Images.Add(image);
                    }
                    HasImages = Images.Count > 0;
                    IsFavorite = await GetIsFavoriteAsync(Item.Id);

                    _media = await _database.GetPoiMediaAsync(Item.Id);
                    HasAudio = _media.Any(m => m.Type == "audio");
                }
                else
                {
                    HasImages = false;
                    IsFavorite = false;
                    HasAudio = false;
                    _media = new List<POI_Media>();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task DownloadOfflineAsync()
        {
            if (Item == null || IsDownloading)
                return;

            try
            {
                IsDownloading = true;
                var images = await _database.GetPoiImagesAsync(Item.Id);
                foreach (var image in images)
                {
                    var localPath = await _offlineContentService.DownloadToCacheAsync(image.ImageUrl, $"poi_{Item.Id}/images");
                    if (!string.IsNullOrWhiteSpace(localPath))
                        image.ImageUrl = localPath;
                }

                var media = await _database.GetPoiMediaAsync(Item.Id);
                foreach (var m in media)
                {
                    if (!string.IsNullOrWhiteSpace(m.AudioUrl))
                    {
                        await _offlineContentService.DownloadToCacheAsync(m.AudioUrl, $"poi_{Item.Id}/audio");
                    }
                }

                Images.Clear();
                foreach (var image in images)
                {
                    Images.Add(image);
                }
                HasImages = Images.Count > 0;

                await Shell.Current.DisplayAlert("Offline", "Đã tải nội dung offline cho POI.", "OK");
            }
            finally
            {
                IsDownloading = false;
            }
        }

        private string GetLocalizedDescription(POI poi)
        {
            if (poi == null)
                return string.Empty;

            var lang = Preferences.Get("Lang", "vi");
            if (lang == "en" && !string.IsNullOrWhiteSpace(poi.DescriptionEn))
                return poi.DescriptionEn;
            return poi.Description;
        }

        private async Task<bool> GetIsFavoriteAsync(int poiId)
        {
            var username = Preferences.Get("Username", string.Empty);
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var user = await _database.GetUserByUsernameAsync(username);
            if (user == null)
                return false;

            return await _database.IsPoiFavoriteAsync(user.Id, poiId);
        }

        [RelayCommand]
        async Task ToggleFavoriteAsync()
        {
            if (Item == null)
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

            await _database.TogglePoiFavoriteAsync(user.Id, Item.Id);
            IsFavorite = await _database.IsPoiFavoriteAsync(user.Id, Item.Id);
        }

        [RelayCommand]
        async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task PlayAudioAsync()
        {
            if (Item != null)
            {
                var audioPlayed = await TryPlayAudioAsync();
                if (!audioPlayed)
                {
                    await _narrationService.SpeakAsync(LocalizedDescription);
                }
            }
        }

        private async Task<bool> TryPlayAudioAsync()
        {
            if (Item == null || _media.Count == 0)
                return false;

            var lang = Preferences.Get("Lang", "vi");
            var audio = _media.FirstOrDefault(m => m.Type == "audio" && m.Language == lang)
                        ?? _media.FirstOrDefault(m => m.Type == "audio");
            if (audio == null || string.IsNullOrWhiteSpace(audio.AudioUrl))
                return false;

            if (_offlineContentService.TryGetCachedPath(audio.AudioUrl, $"poi_{Item.Id}/audio", out var localPath))
            {
                AudioSource = localPath;
                return true;
            }

            AudioSource = audio.AudioUrl;
            return true;
        }

        [RelayCommand]
        async Task OpenDirectionsAsync()
        {
            if (Item == null)
                return;

            var url = $"https://www.google.com/maps/dir/?api=1&destination={Item.Latitude},{Item.Longitude}";
            await Launcher.OpenAsync(new Uri(url));
        }
    }
}
