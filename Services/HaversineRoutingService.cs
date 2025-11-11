using DoctorRoutePlanner.Interfaces;
using DoctorRoutePlanner.Models;

namespace DoctorRoutePlanner.Services
{
    public class HaversineRoutingService : IRoutingService
    {
        private const double EarthRadiusKm = 6371.0;
        private const double AverageSpeedKph = 30.0; // Reduced to more realistic urban speed
        private const double TrafficFactor = 1.3; // Account for traffic

        public double CalculateDistance(Location from, Location to)
        {
            var dLat = ToRadians(to.Latitude - from.Latitude);
            var dLon = ToRadians(to.Longitude - from.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(from.Latitude)) * Math.Cos(ToRadians(to.Latitude)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Math.Round(EarthRadiusKm * c, 2);
        }

        public double CalculateTravelTime(Location from, Location to)
        {
            var distance = CalculateDistance(from, to);
            var timeHours = (distance / AverageSpeedKph) * TrafficFactor;
            return Math.Round(timeHours * 60, 1); // Convert to minutes
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}