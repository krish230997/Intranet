using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Experience
    {

		[Key]
		public int ExperienceId { get; set; }
		public string CompanyName { get; set; }
		public string DesignationName { get; set; }
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }
		[ForeignKey("User")]
		public int UserId { get; set; }
		public User? User { get; set; }
	}
}
