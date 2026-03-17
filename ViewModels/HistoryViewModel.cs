using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Models;

namespace TravelGuideApp.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;

        public ObservableCollection<UserPoiLogItem> Items { get; } = new();

        public HistoryViewModel(SQLiteService database)
        {
            _database = database;
            Title = "Lịch sử tham quan";
        }

        public async Task LoadAsync()
        {
            var username = Preferences.Get("Username", string.Empty);
            if (string.IsNullOrWhiteSpace(username))
                return;

            var user = await _database.GetUserByUsernameAsync(username);
            if (user == null)
                return;

            if (user.Points == 0)
            {
                user.Points = 0;
            }

            Items.Clear();
            var logs = await _database.GetUserPoiLogsAsync(user.Id);
            if (logs.Count == 0)
                return;

            var pois = await _database.GetPOIsAsync();
            var poiMap = pois.ToDictionary(p => p.Id, p => p);

            foreach (var log in logs)
            {
                poiMap.TryGetValue(log.PoiId, out var poi);
                Items.Add(new UserPoiLogItem
                {
                    PoiName = poi?.Name ?? $"POI {log.PoiId}",
                    TriggerType = log.TriggerType,
                    Time = log.StartTime.ToLocalTime()
                });
            }
        }
    }

    public class UserPoiLogItem
    {
        public string PoiName { get; set; }
        public string TriggerType { get; set; }
        public DateTime Time { get; set; }
    }
}
