using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;

namespace TravelGuideApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
