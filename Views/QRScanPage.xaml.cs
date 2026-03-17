using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using TravelGuideApp.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace TravelGuideApp.Views
{
    public partial class QRScanPage : ContentPage
    {
        private readonly QRScanViewModel _viewModel;

        public QRScanPage(QRScanViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Quyền camera", "Vui lòng cấp quyền camera để quét QR.", "OK");
                CameraView.IsDetecting = false;
            }
        }

        private async void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var result = e.Results?.FirstOrDefault();
            if (result == null)
                return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                CameraView.IsDetecting = false;
                await _viewModel.HandleScannedValueAsync(result.Value);
                CameraView.IsDetecting = true;
            });
        }
    }
}
