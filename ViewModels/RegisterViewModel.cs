using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TravelGuideApp.Database;
using TravelGuideApp.Models;
using TravelGuideApp.Views;

namespace TravelGuideApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public RegisterViewModel(SQLiteService database)
        {
            _database = database;
            RegisterCommand = new Command(async () => await OnRegisterAsync());
            BackToLoginCommand = new Command(async () => await OnBackToLoginAsync());
        }

        private async Task OnRegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(PhoneNumber) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin bắt buộc", "OK");
                return;
            }

            if (Password.Length < 8)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Mật khẩu phải từ 8 ký tự trở lên", "OK");
                return;
            }

            if (await _database.GetUserByUsernameAsync(Username) != null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Tên đăng nhập đã tồn tại", "OK");
                return;
            }

            if (await _database.GetUserByEmailAsync(Email) != null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Email đã tồn tại", "OK");
                return;
            }

            if (await _database.GetUserByPhoneAsync(PhoneNumber) != null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Số điện thoại đã tồn tại", "OK");
                return;
            }

            var user = new User
            {
                Username = Username.Trim(),
                Email = Email.Trim(),
                PhoneNumber = PhoneNumber.Trim(),
                FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName.Trim(),
                Role = "Customer",
                Tier = "Normal"
            };

            await _database.CreateUserAsync(user, Password);

            await Application.Current.MainPage.DisplayAlert("Thành công", "Tạo tài khoản thành công. Vui lòng đăng nhập.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        private async Task OnBackToLoginAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
