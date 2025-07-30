using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Promotion
    {
		[Key]
		public int PromotionId { get; set; }

		[Required]
		[ForeignKey(nameof(User))]
		public int UserID { get; set; }

		[Required]
		[StringLength(100)]
		[ForeignKey("Designation")]
		public string DesignationFrom { get; set; }

		[Required]
		[StringLength(100)]
		[ForeignKey("Designation")]
		public string DesignationTo { get; set; }

		[Required]
		public DateTime Date { get; set; }

		// Navigation Property
		public virtual User User { get; set; }
	}
}
