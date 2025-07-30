using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class TrainingType
    {
		[Key]
		public int TrainingTypeId { get; set; }

		[Required]
		public string TrainingTypeName { get; set; }

		[Required]
		public string Description { get; set; }

		[Required]
		public string Status { get; set; }
	}
}
