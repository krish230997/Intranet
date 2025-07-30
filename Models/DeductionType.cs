using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class DeductionType
    {
        [Key]
        public int DeductionTypeId { get; set; } // Primary Key

        [Required]
        [StringLength(100)]
        public string DeductionsName { get; set; }

        // Navigation property
        public virtual ICollection<Deduction> Deductions { get; set; }
    }
}
