using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using TravelGuideApp.Models;
using TravelGuideApp.Database;
using TravelGuideApp.Services;

namespace TravelGuideApp.ViewModels
{
    public partial class ToursViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;
        private readonly SyncService _syncService;

        public ObservableCollection<Tour> Tours { get; } = new();
        public ObservableCollection<Tour> FilteredTours { get; } = new();

        [ObservableProperty]
        string searchQuery;

        [ObservableProperty]
        Tour selectedTour;

        public ToursViewModel(SQLiteService database, SyncService syncService)
        {
            Title = "Available Tours";
            _database = database;
            _syncService = syncService;
        }

        [RelayCommand]
        public async Task LoadToursAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await _syncService.TrySyncToursAsync();
                Tours.Clear();
                FilteredTours.Clear();
                var tours = await _database.GetToursAsync();
                foreach(var tour in tours)
                {
                    Tours.Add(tour);
                }
                foreach (var tour in tours)
                {
                    FilteredTours.Add(tour);
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
            await LoadToursAsync();
        }

        partial void OnSearchQueryChanged(string value)
        {
            ApplyFilter(value);
        }

        private async void ApplyFilter(string value)
        {
            var query = value?.Trim() ?? string.Empty;
            var tours = await _database.GetToursAsync();

            FilteredTours.Clear();
            foreach (var tour in tours)
            {
                if (string.IsNullOrWhiteSpace(query) ||
                    tour.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(tour.Description) && tour.Description.Contains(query, StringComparison.OrdinalIgnoreCase)))
                {
                    FilteredTours.Add(tour);
                }
            }
        }

        partial void OnSelectedTourChanged(Tour value)
        {
            if (value == null)
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Views.TourDetailPage), new Dictionary<string, object>
                {
                    { "tourId", value.Id }
                });
            });

            SelectedTour = null;
        }
    }
}
