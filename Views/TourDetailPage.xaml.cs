using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class TourDetailPage : ContentPage, IQueryAttributable
    {
        private readonly TourDetailViewModel _viewModel;

        public TourDetailPage(TourDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query != null && query.TryGetValue("tourId", out var tourIdObj))
            {
                if (tourIdObj is int tourId)
                {
                    await _viewModel.LoadByIdAsync(tourId);
                }
            }
        }
    }
}
