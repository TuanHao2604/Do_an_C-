using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class HistoryPage : ContentPage
    {
        private readonly HistoryViewModel _viewModel;

        public HistoryPage(HistoryViewModel viewModel)
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
