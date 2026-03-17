using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class FavoritesPage : ContentPage
    {
        private readonly FavoritesViewModel _viewModel;

        public FavoritesPage(FavoritesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}
