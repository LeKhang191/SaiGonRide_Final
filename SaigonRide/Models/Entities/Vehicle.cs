using SaigonRide.Models.Entities;

public class Vehicle
{
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }
}