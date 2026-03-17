using System;
using System.Threading.Tasks;
using Plugin.LocalNotification;

namespace TravelGuideApp.Services
{
    public static class NotificationService
    {
        public static Task<bool> ShowAsync(string title, string description)
        {
            var request = new NotificationRequest
            {
                NotificationId = (int)(DateTime.UtcNow.Ticks % int.MaxValue),
                Title = title,
                Description = description,
                ReturningData = "poi-alert"
            };

            return LocalNotificationCenter.Current.Show(request);
        }
    }
}
