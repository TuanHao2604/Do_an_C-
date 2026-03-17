using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        private const string TokenKey = "ApiToken";
        private const string TokenExpiryKey = "ApiTokenExpiryTicks";

        public SyncService(HttpClient httpClient, SQLiteService database)
        {
            _httpClient = httpClient;
            _database = database;
        }

        public async Task<bool> TrySyncPoisAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return false;

            try
            {
                if (!await EnsureTokenAsync())
                    return false;
                var pois = await _httpClient.GetFromJsonAsync<List<POI>>("api/pois");
                if (pois == null)
                    return false;

                await _database.ReplacePOIsAsync(pois);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> TrySyncPoiAssetsAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return false;

            try
            {
                if (!await EnsureTokenAsync())
                    return false;
                var images = await _httpClient.GetFromJsonAsync<List<POI_Image>>("api/poi-images");
                var media = await _httpClient.GetFromJsonAsync<List<POI_Media>>("api/poi-media");
                if (images != null)
                    await _database.ReplacePoiImagesAsync(images);
                if (media != null)
                    await _database.ReplacePoiMediaAsync(media);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> TrySyncToursAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return false;

            try
            {
                if (!await EnsureTokenAsync())
                    return false;
                var tours = await _httpClient.GetFromJsonAsync<List<Tour>>("api/tours");
                if (tours == null)
                    return false;

                await _database.ReplaceToursAsync(tours);
                return true;
            }
            catch (Exception)
            {
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
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
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
                ClientSecret = ApiSettings.ClientSecret
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/token", request);
            if (!response.IsSuccessStatusCode)
                return false;

            var auth = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();
            if (auth == null || string.IsNullOrWhiteSpace(auth.AccessToken))
                return false;

            Preferences.Set(TokenKey, auth.AccessToken);
            Preferences.Set(TokenExpiryKey, auth.ExpiresAt.ToUniversalTime().Ticks);

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);
            return true;
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
