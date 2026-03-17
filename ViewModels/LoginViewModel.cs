using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TravelGuideApp.Database;
using TravelGuideApp.Views;

namespace TravelGuideApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        private readonly SQLiteService _database;

        public LoginViewModel(SQLiteService database)
        {
            _database = database;
            LoginCommand = new Command(async () => await OnLoginClicked());
            RegisterCommand = new Command(async () => await OnRegisterClicked());
        }

        private async Task OnLoginClicked()
        {
            // Trong thực tế, nên lưu Hashed Password và kiểm tra với Database
            // Ở đây tạm dùng mock logic hoặc Preferences
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin", "OK");
                return;
            }

            var user = await _database.ValidateUserAsync(Username, Password);
            if (user == null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Sai tên đăng nhập hoặc mật khẩu", "OK");
                return;
            }

            Preferences.Set("IsLoggedIn", true);
            Preferences.Set("Username", user.Username);
            Preferences.Set("Role", user.Role ?? "Customer");
            Preferences.Set("Tier", user.Tier ?? "Normal");

            await Application.Current.MainPage.DisplayAlert("Thành công", "Đăng nhập thành công!", "OK");
            await Shell.Current.GoToAsync("//HomePage");
        }
        
        private async Task OnRegisterClicked()
        {
             await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}
