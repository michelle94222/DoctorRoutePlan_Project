namespace DoctorRoutePlanner.Models
{
    public class Appointment
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime TimeWindowStart { get; set; }
        public DateTime TimeWindowEnd { get; set; }
        public int Duration { get; set; } // in minutes
    }

    public class ScheduledAppointment : Appointment
    {
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public double TravelTimeFromPrevious { get; set; } // in minutes
        public double DistanceFromPrevious { get; set; } // in km
    }

    public class RoutePlan
    {
        public List<ScheduledAppointment> Schedule { get; set; } = new();
        public double TotalDistance { get; set; }
        public double TotalTravelTime { get; set; }
        public TimeSpan TotalWorkingTime { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}