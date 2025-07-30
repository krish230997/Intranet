using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class GoalTypeList
    {
		[Key]
		public int GoalId { get; set; }
		[Required]
		public string GoalType { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Status { get; set; }

        //public GoalTrackingList GoalTrackingList { get; set; }
	}
}
