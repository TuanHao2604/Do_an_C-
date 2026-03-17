using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class HomePage : ContentPage
    {
        HomeViewModel _viewModel;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.LoadHomeDataCommand.ExecuteAsync(null);
            }
        }
    }
}
