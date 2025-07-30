using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class PerformanceIndicator
    {
		[Key]
		public int PerformanceIndicatorId { get; set; }
		public string ApprovedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public string Status { get; set; }
		public string CustomerExperience { get; set; }
		public string Marketing { get; set; }
		public string Management { get; set; }
		public string Administration { get; set; }

		public string PresentationSkills { get; set; }
		public string QualityofWork { get; set; }
		public string Efficiency { get; set; }

		public string Integrity { get; set; }
		public string Professionalism { get; set; }
		public string TeamWork { get; set; }
		public string CriticalThinking { get; set; }

		public string ConflictManagement { get; set; }
		public string Attendance { get; set; }
		public string AbilityToMeetDeadline { get; set; }


		[ForeignKey("Department")]
		public int? DepartmentId { get; set; }
		public Department Departments { get; set; }

		[ForeignKey("Designations")]
		public int DesignationId { get; set; }
		public Designation Designations { get; set; }
	}
}
