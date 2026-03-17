using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class ToursPage : ContentPage
    {
        ToursViewModel _viewModel;

        public ToursPage(ToursViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadToursCommand.ExecuteAsync(null);
        }
    }
}
