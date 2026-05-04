using System.ComponentModel.DataAnnotations;

namespace SaigonRide.Models.Entities
{
    public class Station
    {
        [Key]
        public int StationId { get; set; }

        [Required(ErrorMessage = "Station name is required.")]
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Max capacity must be between 1 and 1000.")]
        public int MaxCapacity { get; set; }
        public int CurrentInventory { get; set; }

        // Logic for checking
        public bool IsLowInventory => (double)CurrentInventory / MaxCapacity < 0.2;
    }
}