using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Models;
using TravelGuideApp.Services;

namespace TravelGuideApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;

        private User _user;

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
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

        private string _tier;
        public string Tier
        {
            get => _tier;
            set => SetProperty(ref _tier, value);
        }

        private int _points;
        public int Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GoToLoginCommand { get; }
        public ICommand ToggleLanguageCommand { get; }

        public ProfileViewModel(SQLiteService database)
        {
            _database = database;
            SaveCommand = new Command(async () => await OnSaveAsync());
            LogoutCommand = new Command(async () => await OnLogoutAsync());
            GoToLoginCommand = new Command(async () => await OnGoToLoginAsync());
            ToggleLanguageCommand = new Command(() => ToggleLanguage());
        }

        public async Task LoadAsync()
        {
            var username = Preferences.Get("Username", string.Empty);
            if (string.IsNullOrWhiteSpace(username))
            {
                IsLoggedIn = false;
                return;
            }

            _user = await _database.GetUserByUsernameAsync(username);
            if (_user == null)
            {
                IsLoggedIn = false;
                return;
            }

            Username = _user.Username;
            FullName = _user.FullName;
            Email = _user.Email;
            PhoneNumber = _user.PhoneNumber;
            Tier = _user.Tier ?? "Normal";
            Points = _user.Points;
            IsLoggedIn = true;
        }

        private async Task OnSaveAsync()
        {
            if (_user == null)
                return;

            _user.FullName = FullName?.Trim();
            _user.Email = Email?.Trim();
            _user.PhoneNumber = PhoneNumber?.Trim();
            _user.Tier = Tier;
            _user.Points = Points;

            await _database.UpdateUserAsync(_user);
            await Application.Current.MainPage.DisplayAlert("Thành công", "Đã cập nhật thông tin.", "OK");
        }

        private async Task OnLogoutAsync()
        {
            Preferences.Remove("IsLoggedIn");
            Preferences.Remove("Username");
            Preferences.Remove("Role");
            Preferences.Remove("Tier");

            IsLoggedIn = false;
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async Task OnGoToLoginAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private void ToggleLanguage()
        {
            var current = LocalizationService.CurrentLanguage;
            var next = current == "vi" ? "en" : "vi";
            LocalizationService.SetLanguage(next);
            Application.Current.MainPage.DisplayAlert("Ngôn ngữ", $"Đã chuyển sang {next.ToUpper()}.", "OK");
        }
    }
}
