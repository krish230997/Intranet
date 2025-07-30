using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
	public class AddTrainingViewModel
	{
		[Key]
		public int TrainingId { get; set; }  // Add this property

		[Required]
		public int TrainerId { get; set; }

		[Required]
		public int TrainingTypeId { get; set; }

		[Required]
		public int UserId { get; set; }

		[Required]
		[Range(0, double.MaxValue, ErrorMessage = "Training cost must be a positive value.")]
		public decimal TrainingCost { get; set; }

		public string Description { get; set; }

		[Required]
		public string Status { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }

	}
}
