using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Earning
    {
        [Key]
        public int EarningsId { get; set; }

        // Foreign Key for EarningType
        [ForeignKey("EarningType")]
        public int EarntypeId { get; set; }
        public EarningType EarningType { get; set; }

        public decimal EarningsPercentage { get; set; }

        // Foreign Key for Department
        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // Foreign Key for Designation
        [ForeignKey("Designation")]
        public int DesignationId { get; set; }
        public Designation Designation { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

}
