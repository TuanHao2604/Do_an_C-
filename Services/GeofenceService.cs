using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Devices.Sensors;
using TravelGuideApp.Models;

namespace TravelGuideApp.Services
{
    public class GeofenceService
    {
        // Distance is in meters
        public double CalculateDistance(Location loc1, Location loc2)
        {
            return Location.CalculateDistance(loc1, loc2, DistanceUnits.Kilometers) * 1000;
        }

        public POI CheckGeofence(Location currentLocation, List<POI> pois)
        {
            foreach (var poi in pois.OrderBy(p => p.Priority))
            {
                var poiLocation = new Location(poi.Latitude, poi.Longitude);
                double distance = CalculateDistance(currentLocation, poiLocation);
                
                if (distance <= poi.Radius)
                {
                    return poi;
                }
            }
            return null;
        }
    }
}
