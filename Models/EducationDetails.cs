using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class EducationDetails
    {
        [Key]
        public int EducationDetailsId { get; set; }
        public string EducationType { get; set; }
        public string UniversityName { get; set; }
		public DateTime? startdate { get; set; }
		public DateTime? enddate { get; set; }
		[ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    

}
