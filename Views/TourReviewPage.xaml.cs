using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class TourReviewPage : ContentPage, IQueryAttributable
    {
        private readonly TourReviewViewModel _viewModel;

        public TourReviewPage(TourReviewViewModel viewModel)
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
                    await _viewModel.LoadAsync(tourId);
                }
            }
        }
    }
}
