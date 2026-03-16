using System.ComponentModel.DataAnnotations;

namespace Pethub.Models
{
    public class Account
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Contact{ get; set; }

        [Required]
        public string Birthday { get; set; }
        public int Age { get; set; }

    }
}
