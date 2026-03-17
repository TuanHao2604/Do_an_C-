using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Models;

namespace TravelGuideApp.Services
{
    public class SyncService
    {
        private readonly HttpClient _httpClient;
        private readonly SQLiteService _database;
        private readonly ILogger<SyncService> _logger;
        private const string TokenKey = "ApiToken";
        private const string TokenExpiryKey = "ApiTokenExpiryTicks";

        public SyncService(HttpClient httpClient, SQLiteService database, ILogger<SyncService> logger)
        {
            _httpClient = httpClient;
            _database = database;
            _logger = logger;
        }

        public async Task<bool> TrySyncPoisAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                _logger.LogDebug("SyncPOIs skipped: no internet connection.");
                return false;
            }

            try
            {
                if (!await EnsureTokenAsync())
                {
                    _logger.LogWarning("SyncPOIs: failed to obtain API token.");
                    return false;
                }

                var pois = await _httpClient.GetFromJsonAsync<List<POI>>("api/pois");
                if (pois == null)
                {
                    _logger.LogWarning("SyncPOIs: server returned null.");
                    return false;
                }

                await _database.ReplacePOIsAsync(pois);
                _logger.LogInformation("SyncPOIs: synced {Count} POIs.", pois.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncPOIs failed.");
                return false;
            }
        }

        /// <summary>
        /// Sync POI images và media theo cách atomic:
        /// chỉ ghi vào DB khi cả hai đều fetch thành công.
        /// </summary>
        public async Task<bool> TrySyncPoiAssetsAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                _logger.LogDebug("SyncPoiAssets skipped: no internet connection.");
                return false;
            }

            try
            {
                if (!await EnsureTokenAsync())
                {
                    _logger.LogWarning("SyncPoiAssets: failed to obtain API token.");
                    return false;
                }

                // Fetch cả hai trước — nếu một trong hai thất bại thì không ghi gì cả
                var images = await _httpClient.GetFromJsonAsync<List<POI_Image>>("api/poi-images");
                var media  = await _httpClient.GetFromJsonAsync<List<POI_Media>>("api/poi-media");

                if (images == null || media == null)
                {
                    _logger.LogWarning("SyncPoiAssets: server returned null for images or media.");
                    return false;
                }

                // Atomic: chỉ replace khi cả hai đã có dữ liệu hợp lệ
                await _database.ReplacePoiImagesAsync(images);
                await _database.ReplacePoiMediaAsync(media);

                _logger.LogInformation("SyncPoiAssets: synced {ImgCount} images, {MediaCount} media.", images.Count, media.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncPoiAssets failed.");
                return false;
            }
        }

        public async Task<bool> TrySyncToursAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                _logger.LogDebug("SyncTours skipped: no internet connection.");
                return false;
            }

            try
            {
                if (!await EnsureTokenAsync())
                {
                    _logger.LogWarning("SyncTours: failed to obtain API token.");
                    return false;
                }

                var tours = await _httpClient.GetFromJsonAsync<List<Tour>>("api/tours");
                if (tours == null)
                {
                    _logger.LogWarning("SyncTours: server returned null.");
                    return false;
                }

                await _database.ReplaceToursAsync(tours);
                _logger.LogInformation("SyncTours: synced {Count} tours.", tours.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncTours failed.");
                return false;
            }
        }

        public async Task<bool> TryPostUserPoiLogAsync(string username, int poiId, string triggerType, DateTime timestampUtc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return false;

            try
            {
                if (!await EnsureTokenAsync())
                    return false;

                var payload = new UserPoiLogPayload
                {
                    Username = username,
                    PoiId = poiId,
                    TriggerType = triggerType,
                    StartTime = timestampUtc,
                    EndTime = timestampUtc
                };

                var response = await _httpClient.PostAsJsonAsync("api/logs", payload);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PostUserPoiLog failed: HTTP {StatusCode}.", (int)response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostUserPoiLog failed.");
                return false;
            }
        }

        private async Task<bool> EnsureTokenAsync()
        {
            var token = Preferences.Get(TokenKey, string.Empty);
            var expiryTicks = Preferences.Get(TokenExpiryKey, 0L);
            if (!string.IsNullOrWhiteSpace(token) && expiryTicks > DateTime.UtcNow.AddMinutes(1).Ticks)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return true;
            }

            var request = new ApiAuthRequest
            {
                ClientId = ApiSettings.ClientId,
                ClientSecret = await ApiSettings.GetClientSecretAsync()
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/token", request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("EnsureToken: token request failed with HTTP {StatusCode}.", (int)response.StatusCode);
                    return false;
                }

                var auth = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();
                if (auth == null || string.IsNullOrWhiteSpace(auth.AccessToken))
                {
                    _logger.LogWarning("EnsureToken: received empty token response.");
                    return false;
                }

                Preferences.Set(TokenKey, auth.AccessToken);
                Preferences.Set(TokenExpiryKey, auth.ExpiresAt.ToUniversalTime().Ticks);

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EnsureToken request threw exception.");
                return false;
            }
        }

        private class UserPoiLogPayload
        {
            public string Username { get; set; }
            public int PoiId { get; set; }
            public string TriggerType { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        private class ApiAuthRequest
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        private class ApiAuthResponse
        {
            public string AccessToken { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
