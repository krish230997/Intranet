using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Attendance
    {
		[Key]
		public int AttendanceId { get; set; }

		[ForeignKey("User")]
		public int UserId { get; set; }

		[Required]
		public DateTime Date { get; set; }

		public DateTime? CheckIn { get; set; }

		public DateTime? CheckOut { get; set; }

		public DateTime? LunchIn { get; set; }

		public DateTime? LunchOut { get; set; }

		public decimal WorkingHours { get; set; }

		public decimal ProductionHours { get; set; }

		public decimal OvertimeHours { get; set; }

		public decimal BreakHours { get; set; }

		public int Late { get; set; }

		public string Status { get; set; }

		public User User { get; set; }

	}
}
