using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaigonRide.Models.Entities
{
    public class Rental
    {
        [Key]
        public int RentalId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int StartStationId { get; set; }

        public int? EndStationId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public double? BaseFare { get; set; }

        public double? FinalFare { get; set; }

        public bool DiscountApplied { get; set; } = false;

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public virtual User? User { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual Station? StartStation { get; set; }
        public virtual Station? EndStation { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}