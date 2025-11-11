# Doctor Route Planner

An ASP.NET Core 8+ web application that helps doctors optimize their house call schedules by generating the most efficient routes while respecting appointment time constraints.

## Features

- Smart Route Planning: Generates optimal routes for house calls starting and ending at home
- Time Window Management: Respects patient appointment time windows with intelligent waiting
- Geographic Calculations: Uses Haversine formula for distance calculations (no external APIs required)
- Responsive UI: Clean Bootstrap-based interface for easy use on any device
- Flexible Input: Accepts JSON data via text input or file upload
- Extensible Architecture: Interface-based design for easy testing and enhancements

## Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or any .NET-compatible IDE

### Installation

1. Run the application
   dotnet run
2. Open your browser
   Navigate to https://localhost:5000

### Usage

1. Set Home Location: Enter the doctor's starting coordinates (default: New York City)
2. Input Appointments: Paste JSON data or upload a file with appointment details
3. Generate Route: Click the button to calculate the optimal schedule
4. View Results: See the optimized route with timing and travel information

## Sample Input Format

[
  {
    "patientId": "1",
    "patientName": "Michelle Zhang",
    "latitude": 40.7505,
    "longitude": -73.9934,
    "timeWindowStart": "2025-12-01T09:00:00",
    "timeWindowEnd": "2025-12-01T11:00:00",
    "duration": 30
  },
  {
    "patientId": "2",
    "patientName": "John Tito",
    "latitude": 40.7589,
    "longitude": -73.9851,
    "timeWindowStart": "2025-12-01T10:00:00",
    "timeWindowEnd": "2025-12-01T12:00:00",
    "duration": 45
  }
]

## Project Structure

DoctorRoutePlanner/
├── Pages/                 # Razor Pages UI
├── Models/               # Data models (Appointment, RoutePlan, etc.)
├── Services/             # Business logic (RoutePlanner, Routing)
├── Interfaces/           # Contracts for dependency injection
├── Utils/                # Helper classes
└── wwwroot/              # Static files (CSS, JS)

## Key Components

### Models
- Appointment: Patient details, location, and time constraints
- ScheduledAppointment: Appointment with calculated timing
- RoutePlan: Complete optimized schedule with totals

### Services
- RoutePlannerService: Core algorithm for finding optimal routes
- HaversineRoutingService: Distance and travel time calculations

### Algorithms
- Greedy Scheduling: Prioritizes appointments by time windows
- Feasibility Checking: Ensures appointments can be completed within constraints
- Travel Optimization: Minimizes total distance and travel time

## Technology Stack

- Framework: ASP.NET Core 8.0
- UI: Razor Pages with Bootstrap 5
- Language: C# 12.0
- Architecture: Clean architecture with dependency injection
- Calculations: Haversine formula for geospatial math

## Configuration

The application uses sensible defaults:
- Start Time: 8:00 AM
- Average Speed: 30 km/h (urban driving)
- Traffic Factor: 1.3x (accounts for urban traffic conditions)

## API Documentation

### Input Schema
public class Appointment
{
    public string PatientId { get; set; }
    public string PatientName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime TimeWindowStart { get; set; }
    public DateTime TimeWindowEnd { get; set; }
    public int Duration { get; set; } // minutes
}

### Output Schema
public class RoutePlan
{
    public List<ScheduledAppointment> Schedule { get; set; }
    public double TotalDistance { get; set; } // kilometers
    public double TotalTravelTime { get; set; } // minutes
    public TimeSpan TotalWorkingTime { get; set; }
}

## Contributing

1. Fork the repository
2. Create a feature branch (git checkout -b feature/amazing-feature)
3. Commit your changes (git commit -m 'Add amazing feature')
4. Push to the branch (git push origin feature/amazing-feature)
5. Open a Pull Request

## Future Enhancements

- Real-time traffic integration
- Multiple optimization strategies
- Doctor break scheduling
- Fuel cost calculations
- Mobile app companion
- Historical performance analytics
