using Microsoft.Maui.Controls;
using TravelGuideApp.Models;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class AudioGuidePage : ContentPage
    {
        public AudioGuidePage(AudioGuideViewModel viewModel, POI poi)
        {
            InitializeComponent();
            BindingContext = viewModel;
            viewModel.Init(poi);
        }
    }
}
