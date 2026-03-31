using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    public class Pet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pet name is required.")]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Species is required.")]
        public string Species { get; set; } = null!; // e.g., Dog, Cat, Bird

        public string? Breed { get; set; }

        public int Age { get; set; }

        public string? VaccinationStatus { get; set; } // e.g., Fully Vaccinated, Unvaccinated

        // Kept as nvarchar(max) or 2048 to prevent DB crashes
        [MaxLength(2048)]
        public string? ImagePath { get; set; }

        // Foreign Key to link the pet to the user who owns it
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;
    }
}