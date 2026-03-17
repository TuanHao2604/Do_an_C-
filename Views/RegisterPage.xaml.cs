using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
