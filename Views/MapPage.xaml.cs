using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class MapPage : ContentPage
    {
        MapViewModel _viewModel;

        public MapPage(MapViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.LoadDataCommand.ExecuteAsync(null);
                await _viewModel.StartTrackingCommand.ExecuteAsync(null);
                UpdatePins();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel?.StopTrackingCommand.Execute(null);
        }

        private void UpdatePins()
        {
            if (_viewModel == null || PoiMap == null)
                return;

            PoiMap.Pins.Clear();
            foreach (var poi in _viewModel.PointsOfInterest)
            {
                var location = new Location(poi.Latitude, poi.Longitude);
                PoiMap.Pins.Add(new Pin
                {
                    Location = location,
                    Label = poi.Name,
                    Address = poi.Description
                });
            }
        }
    }
}
