using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using TravelGuideApp.Database;
using TravelGuideApp.Models;

namespace TravelGuideApp.ViewModels
{
    public class TourReviewViewModel : BaseViewModel
    {
        private readonly SQLiteService _database;

        public ObservableCollection<Review> Reviews { get; } = new();

        private int _tourId;

        private int _rating;
        public int Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        private string _comment;
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        public ICommand SubmitCommand { get; }

        public TourReviewViewModel(SQLiteService database)
        {
            _database = database;
            SubmitCommand = new Command(async () => await SubmitAsync());
        }

        public async Task LoadAsync(int tourId)
        {
            _tourId = tourId;
            Reviews.Clear();
            var items = await _database.GetReviewsForTourAsync(tourId);
            foreach (var item in items)
                Reviews.Add(item);
        }

        private async Task SubmitAsync()
        {
            if (_tourId == 0)
                return;

            if (Rating < 1 || Rating > 5)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng chọn số sao từ 1 đến 5.", "OK");
                return;
            }

            var username = Preferences.Get("Username", string.Empty);
            if (string.IsNullOrWhiteSpace(username))
            {
                await Application.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng đăng nhập để đánh giá.", "OK");
                return;
            }

            var user = await _database.GetUserByUsernameAsync(username);
            if (user == null)
                return;

            var review = new Review
            {
                UserId = user.Id,
                TourId = _tourId,
                Rating = Rating,
                Comment = Comment
            };

            await _database.AddReviewAsync(review);
            Rating = 0;
            Comment = string.Empty;
            await LoadAsync(_tourId);
        }
    }
}
