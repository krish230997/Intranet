using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Trainer
    {
		[Key]
		public int TrainerId { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		public string Role { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		public string Description { get; set; }

		[Required]
		public string Status { get; set; }

        public string? ProfilePicture { get; set; }

        [NotMapped]
        public IFormFile? imagename { get; set; }

        public long Phone { get; set; }


    }
}
