using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Timesheet
    {
		[Key]
		public int TimesheetId { get; set; }

		[ForeignKey("User")]
		public int UserId { get; set; }
		public User User { get; set; }

		public DateTime Date { get; set; }
		public int WorkHours { get; set; }
		public string Status { get; set; }

		public string CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public string? ApprovedBy { get; set; }
		public DateTime? ApprovedAt { get; set; }

		[ForeignKey("Projects")]
		public int ProjectId { get; set; }
		public Projects Projects { get; set; }


	}
}
