using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Designation
    {
        [Key]
        public int DesignationId { get; set; }

        [ForeignKey("Department")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }
        [Required(ErrorMessage = "Designation Name is required.")]
        public string Name { get; set; }
        public int? NoOfEmployee { get; set; }
        [Required(ErrorMessage = "Status is required.")]
        public string status { get; set; }
        public DateTime? CreatedAt { get; set; }/* = DateTime.UtcNow;*/
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public List<Earning> Earnings { get; set; }
        public List<Deduction> Deductions { get; set; }


    }
}
