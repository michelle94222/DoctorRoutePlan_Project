using DoctorRoutePlanner.Models;

namespace DoctorRoutePlanner.Interfaces
{
    public interface IRoutePlannerService
    {
        RoutePlan? GenerateOptimalRoute(List<Appointment> appointments, Location homeLocation);
    }
}