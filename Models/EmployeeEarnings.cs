using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class EmployeeEarnings
    {
        [Key]
        public int EmployeeEarningId { get; set; }

        [ForeignKey("EmployeeSalaries")]
        public int SalaryId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Earning")]
        public int EarningId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EarningAmount { get; set; }

        // Navigation properties
        public virtual EmployeeSalaries EmployeeSalaries { get; set; }
        public virtual Earning Earning { get; set; }
    }
}
