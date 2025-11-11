using DoctorRoutePlanner.Models;

namespace DoctorRoutePlanner.Interfaces
{
    public interface IRoutingService
    {
        double CalculateDistance(Location from, Location to);
        double CalculateTravelTime(Location from, Location to);
    }
}