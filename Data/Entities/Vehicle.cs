using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace garage3.Data
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required, MaxLength(12)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Color { get; set; }

        [MaxLength(50)]
        public string? Manufacturer { get; set; }

        [MaxLength(50)]
        public string? Model { get; set; }

        // FK -> AspNetUsers(ApplicationUser)
        [Required]
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser Owner { get; set; } = null!;

        // FK -> VehicleType
        public int VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // historik
        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
