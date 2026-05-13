using Microsoft.EntityFrameworkCore;
using SaigonRide.Models.Entities;

namespace SaigonRide.Models.Entities
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public int StationId { get; set; }
        public Station? Station { get; set; }
        public int? BatteryLevel { get; set; }
        public string? ImageUrl { get; set; }
    }
}