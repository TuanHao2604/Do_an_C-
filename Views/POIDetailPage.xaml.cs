using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using TravelGuideApp.Models;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class POIDetailPage : ContentPage, IQueryAttributable
    {
        private readonly POIViewModel _viewModel;

        public POIDetailPage(POIViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext == null)
            {
                BindingContext = _viewModel;
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query != null && query.TryGetValue("poiId", out var poiIdObj))
            {
                if (poiIdObj is int poiId)
                {
                    await _viewModel.LoadByIdAsync(poiId);
                    UpdateMapForPoi(_viewModel.Item);
                }
            }
        }

        private void UpdateMapForPoi(POI poi)
        {
            if (poi == null || PoiMap == null)
                return;

            var location = new Location(poi.Latitude, poi.Longitude);
            PoiMap.Pins.Clear();
            PoiMap.Pins.Add(new Pin
            {
                Location = location,
                Label = poi.Name,
                Address = poi.Description
            });
            PoiMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                location,
                Distance.FromKilometers(1)));
        }
    }
}
