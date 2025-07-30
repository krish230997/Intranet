using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
	public class Activitys
	{
		[Key]
		public int ActivityId { get; set; }

		[Required]
		[StringLength(100)]
		public string Title { get; set; }

		[Required]
		[StringLength(50)]
		public string ActivityType { get; set; }

		[Required]
		public DateTime DueDate { get; set; }

		[Required]
		[StringLength(100)]
		public string Owner { get; set; }

		[Required]
		public DateTime CreatedDate { get; set; }

		public string Description { get; set; }

		[StringLength(50)]
		public string Status { get; set; }
	}
}
