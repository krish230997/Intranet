using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Pulse360.Models
{
    public class GoalTrackingList
    {
		[Key]
		public int GoalTrackingId { get; set; }
		[Required]
		public string Subject { get; set; }
        [Required]
        public string TargetAchievement { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Status { get; set; }

		[ForeignKey("GoalTypeList")]
		public int GoalId { get; set; }
        [AllowNull]
        public GoalTypeList? GoalTypeList { get; set; }
	}
}
