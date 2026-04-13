using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    // Represents a pet owned by a user
    public class Pet
    {
        // Unique ID for each pet
        public int Id { get; set; }

        // Pet name is required and limited to 50 characters
        [Required(ErrorMessage = "Pet name is required.")]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        // Species is required, like Dog or Cat
        [Required(ErrorMessage = "Species is required.")]
        public string Species { get; set; } = null!;

        // Breed is optional
        public string? Breed { get; set; }

        // Age is stored as a number
        public int Age { get; set; }

        // Vaccination status is optional
        public string? VaccinationStatus { get; set; }

        // Path to the pet’s image, limited to 2048 characters
        [MaxLength(2048)]
        public string? ImagePath { get; set; }

        // Links the pet to the account that owns it
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;
    }
}
