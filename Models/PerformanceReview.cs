using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class PerformanceReview
    {
		[Key]
		public int ReviewId { get; set; }
		public string Name { get; set; }
		public string Department { get; set; }

		public int EmployeeId { get; set; }
		public string Designation { get; set; }

		public DateTime DateofJoin { get; set; }
		public string ROName { get; set; }

		public DateTime DateofConfirmation { get; set; }

		public string RODesignation { get; set; }
		public string Qualification { get; set; }

		public int PreviousyearsofExp { get; set; }
	}
}
