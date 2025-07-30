using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Termination
    {
		[Key]
		public int TerminationId { get; set; }

		[Required]
		[ForeignKey("User")]
		public int UserID { get; set; }

		[Required]
		[StringLength(100)]
		public string TerminationType { get; set; }

		[Required]
		public DateTime NoticeDate { get; set; }

		[Required]
		public DateTime ResignDate { get; set; }

		[Required]
		[StringLength(500)]
		public string Reason { get; set; }

		// Navigation Property
		public virtual User User { get; set; }

	}
}
