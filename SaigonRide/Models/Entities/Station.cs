namespace SaigonRide.Models.Entities
{
    public class Station
    {
        public int StationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public int CurrentInventory { get; set; }

        // Logic for checking
        public bool IsLowInventory => CurrentInventory < (MaxCapacity * 0.2);
    }
}