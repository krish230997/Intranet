using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        [Required(ErrorMessage = "Department Name is required.")]
        public string Name { get; set; }
        public int? NoOfEmployee { get; set; }
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }/* = DateTime.UtcNow;*/
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<Designation> Designations { get; set; }

        public List<DepartmentLeaves> DepartmentLeaves { get; set; }

        public List<Earning> Earnings { get; set; }
        public List<Deduction> Deductions { get; set; }

        //public List<User> Users { get; set; }

    }
}
