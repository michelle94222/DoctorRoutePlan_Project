using DoctorRoutePlanner.Interfaces;
using DoctorRoutePlanner.Models;

namespace DoctorRoutePlanner.Services
{
    public class RoutePlannerService : IRoutePlannerService
    {
        private readonly IRoutingService _routingService;
        private readonly ILogger<RoutePlannerService> _logger;

        public RoutePlannerService(IRoutingService routingService, ILogger<RoutePlannerService> logger)
        {
            _routingService = routingService;
            _logger = logger;
        }

        public RoutePlan? GenerateOptimalRoute(List<Appointment> appointments, Location homeLocation)
        {
            if (!appointments.Any()) 
                return new RoutePlan { Schedule = new List<ScheduledAppointment>() };

            // Filter out impossible appointments and log warnings
            var validAppointments = appointments.Where(a => 
            {
                var isValid = (a.TimeWindowEnd - a.TimeWindowStart).TotalMinutes >= a.Duration;
                if (!isValid)
                {
                    _logger.LogWarning($"Appointment for {a.PatientName} skipped: duration exceeds time window");
                }
                return isValid;
            }).ToList();

            if (!validAppointments.Any())
            {
                _logger.LogWarning("No valid appointments after filtering");
                return null;
            }

            // Use a more robust algorithm that can handle time constraints better
            var bestRoute = FindFeasibleRoute(validAppointments, homeLocation);
            return bestRoute;
        }

        private RoutePlan? FindFeasibleRoute(List<Appointment> appointments, Location homeLocation)
        {
            // Start with appointments sorted by time window start (earliest first)
            var sortedAppointments = appointments
                .OrderBy(a => a.TimeWindowStart)
                .ThenBy(a => a.TimeWindowEnd)
                .ToList();

            var scheduledAppointments = new List<ScheduledAppointment>();
            var currentTime = DateTime.Today.AddHours(8); // Start at 8 AM
            Location currentLocation = homeLocation;
            double totalDistance = 0;
            double totalTravelTime = 0;

            var remainingAppointments = new List<Appointment>(sortedAppointments);

            while (remainingAppointments.Any())
            {
                var nextAppointment = FindNextFeasibleAppointment(remainingAppointments, currentLocation, currentTime);

                if (nextAppointment == null)
                {
                    // If no appointment fits, try to insert the one with the earliest deadline
                    nextAppointment = remainingAppointments
                        .OrderBy(a => a.TimeWindowEnd)
                        .FirstOrDefault();
                    
                    if (nextAppointment == null) break;
                }

                // Calculate travel to this appointment
                var travelTime = _routingService.CalculateTravelTime(currentLocation, 
                    new Location(nextAppointment.Latitude, nextAppointment.Longitude));
                var distance = _routingService.CalculateDistance(currentLocation,
                    new Location(nextAppointment.Latitude, nextAppointment.Longitude));

                // Arrival time considering travel
                var arrivalTime = currentTime.AddMinutes(travelTime);

                // If we arrive too early, wait until the time window starts
                if (arrivalTime < nextAppointment.TimeWindowStart)
                {
                    arrivalTime = nextAppointment.TimeWindowStart;
                }

                // Calculate departure time
                var departureTime = arrivalTime.AddMinutes(nextAppointment.Duration);

                // If we can't complete within time window, adjust arrival time to fit
                if (departureTime > nextAppointment.TimeWindowEnd)
                {
                    // Try to arrive earlier to complete on time
                    var latestArrivalTime = nextAppointment.TimeWindowEnd.AddMinutes(-nextAppointment.Duration);
                    if (latestArrivalTime >= currentTime.AddMinutes(travelTime))
                    {
                        arrivalTime = latestArrivalTime;
                        departureTime = nextAppointment.TimeWindowEnd;
                    }
                    else
                    {
                        // This appointment cannot be scheduled - skip it
                        _logger.LogWarning($"Cannot schedule {nextAppointment.PatientName} - no time available");
                        remainingAppointments.Remove(nextAppointment);
                        continue;
                    }
                }

                var scheduledAppointment = new ScheduledAppointment
                {
                    PatientId = nextAppointment.PatientId,
                    PatientName = nextAppointment.PatientName,
                    Latitude = nextAppointment.Latitude,
                    Longitude = nextAppointment.Longitude,
                    TimeWindowStart = nextAppointment.TimeWindowStart,
                    TimeWindowEnd = nextAppointment.TimeWindowEnd,
                    Duration = nextAppointment.Duration,
                    ArrivalTime = arrivalTime,
                    DepartureTime = departureTime,
                    TravelTimeFromPrevious = travelTime,
                    DistanceFromPrevious = distance
                };

                scheduledAppointments.Add(scheduledAppointment);
                totalDistance += distance;
                totalTravelTime += travelTime;
                currentTime = departureTime;
                currentLocation = new Location(nextAppointment.Latitude, nextAppointment.Longitude);
                remainingAppointments.Remove(nextAppointment);
            }

            if (!scheduledAppointments.Any())
                return null;

            // Calculate return to home
            var returnTravelTime = _routingService.CalculateTravelTime(currentLocation, homeLocation);
            var returnDistance = _routingService.CalculateDistance(currentLocation, homeLocation);
            totalDistance += returnDistance;
            totalTravelTime += returnTravelTime;

            var endTime = scheduledAppointments.Last().DepartureTime.AddMinutes(returnTravelTime);
            var startTime = DateTime.Today.AddHours(8);

            return new RoutePlan
            {
                Schedule = scheduledAppointments,
                TotalDistance = Math.Round(totalDistance, 2),
                TotalTravelTime = Math.Round(totalTravelTime, 2),
                TotalWorkingTime = endTime - startTime
            };
        }

        private Appointment? FindNextFeasibleAppointment(List<Appointment> appointments, Location currentLocation, DateTime currentTime)
        {
            foreach (var appointment in appointments.OrderBy(a => a.TimeWindowStart))
            {
                var travelTime = _routingService.CalculateTravelTime(currentLocation,
                    new Location(appointment.Latitude, appointment.Longitude));
                
                var earliestArrival = currentTime.AddMinutes(travelTime);
                var latestArrival = appointment.TimeWindowEnd.AddMinutes(-appointment.Duration);

                // If we can arrive before the latest possible time
                if (earliestArrival <= latestArrival)
                {
                    return appointment;
                }
            }
            return null;
        }
    }
}