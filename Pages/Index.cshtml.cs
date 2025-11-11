using DoctorRoutePlanner.Interfaces;
using DoctorRoutePlanner.Models;
using DoctorRoutePlanner.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace DoctorRoutePlanner.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IRoutePlannerService _routePlannerService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IRoutePlannerService routePlannerService, ILogger<IndexModel> logger)
        {
            _routePlannerService = routePlannerService;
            _logger = logger;
        }

        [BindProperty]
        public string JsonInput { get; set; } = string.Empty;

        [BindProperty]
        public double HomeLatitude { get; set; } = 40.7128;

        [BindProperty]
        public double HomeLongitude { get; set; } = -74.0060;

        public RoutePlan? RoutePlan { get; set; }
        public string? ErrorMessage { get; set; }
        public string? JsonValidationError { get; set; }

        public void OnGet()
        {
            // Set sample data with correct format
            JsonInput = @"[
  {
    ""patientId"": ""1"",
    ""patientName"": ""John Doe"",
    ""latitude"": 40.7505,
    ""longitude"": -73.9934,
    ""timeWindowStart"": ""2025-12-01T09:00:00"",
    ""timeWindowEnd"": ""2025-12-01T11:00:00"",
    ""duration"": 30
  },
  {
    ""patientId"": ""2"", 
    ""patientName"": ""Jane Smith"",
    ""latitude"": 40.7589,
    ""longitude"": -73.9851,
    ""timeWindowStart"": ""2025-12-01T10:00:00"",
    ""timeWindowEnd"": ""2025-12-01T12:00:00"",
    ""duration"": 45
  }
]";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var jsonData = JsonInput;
                var file = Request.Form.Files["file"];

                // Read from file if uploaded
                if (file != null && file.Length > 0)
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    jsonData = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    ErrorMessage = "Please provide JSON data or upload a file.";
                    return Page();
                }

                // Validate JSON syntax first
                if (!IsValidJson(jsonData, out var validationError))
                {
                    JsonValidationError = validationError;
                    ErrorMessage = "Invalid JSON format. Please check your input.";
                    return Page();
                }

                // Parse appointments
                if (!JsonHelper.TryParseJson<List<Appointment>>(jsonData, out var appointments) || appointments == null || !appointments.Any())
                {
                    ErrorMessage = "Invalid JSON format or no appointments found. Please check your input.";
                    return Page();
                }

                // Validate appointment data
                var validationResults = ValidateAppointments(appointments);
                if (!validationResults.IsValid)
                {
                    ErrorMessage = validationResults.ErrorMessage;
                    return Page();
                }

                // Generate route
                var homeLocation = new Location(HomeLatitude, HomeLongitude);
                RoutePlan = _routePlannerService.GenerateOptimalRoute(appointments, homeLocation);

                if (RoutePlan == null)
                {
                    ErrorMessage = "Unable to generate a valid route. This could be because: \n" +
                                  "- The time windows are too tight for the travel times\n" +
                                  "- Appointment durations exceed their time windows\n" +
                                  "- The locations are too far apart for the given time constraints";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating route");
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }

        private bool IsValidJson(string json, out string error)
        {
            error = string.Empty;
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateAppointments(List<Appointment> appointments)
        {
            foreach (var appointment in appointments)
            {
                // Check if duration fits within time window
                if (appointment.Duration <= 0)
                {
                    return (false, $"Appointment for {appointment.PatientName} has invalid duration: {appointment.Duration} minutes");
                }

                var timeWindowLength = (appointment.TimeWindowEnd - appointment.TimeWindowStart).TotalMinutes;
                if (appointment.Duration > timeWindowLength)
                {
                    return (false, $"Appointment for {appointment.PatientName} has duration ({appointment.Duration}min) longer than time window ({timeWindowLength}min)");
                }

                // Check valid coordinates
                if (appointment.Latitude < -90 || appointment.Latitude > 90 || 
                    appointment.Longitude < -180 || appointment.Longitude > 180)
                {
                    return (false, $"Appointment for {appointment.PatientName} has invalid coordinates");
                }

                // Check time window order
                if (appointment.TimeWindowStart >= appointment.TimeWindowEnd)
                {
                    return (false, $"Appointment for {appointment.PatientName} has time window end before start");
                }
            }

            return (true, string.Empty);
        }
    }
}