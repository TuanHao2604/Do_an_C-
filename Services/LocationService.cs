using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

namespace TravelGuideApp.Services
{
    public class LocationService
    {
        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status != PermissionStatus.Granted)
                {
                    // Unable to get location
                    return null;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);
                return location;
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine($"Error getting location: {ex.Message}");
                return null;
            }
        }

        public async Task ListenForLocationUpdatesAsync(Func<Location, Task> onUpdate, TimeSpan interval, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var location = await GetCurrentLocationAsync();
                if (location != null && onUpdate != null)
                {
                    await onUpdate(location);
                }

                try
                {
                    await Task.Delay(interval, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
