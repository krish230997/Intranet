using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Resignation
    {
        [Key]
        public int ResignationId { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        public int UserID { get; set; }  // Ensure this is set in the controller

        [Required(ErrorMessage = "Department is required.")]
        public int DepartmentId { get; set; }  // Ensure this is set in the controller

        [Required(ErrorMessage = "Notice date is required.")]
        [DataType(DataType.Date)]
        public DateTime NoticeDate { get; set; }

        [Required(ErrorMessage = "Resignation date is required.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Resignation), nameof(ValidateResignDate))]
        public DateTime ResignDate { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason must be between 10 and 500 characters.", MinimumLength = 10)]
        public string Reason { get; set; }

        // ✅ Navigation Properties (Should NOT be required)
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        // ✅ Custom Validation
        public static ValidationResult ValidateResignDate(DateTime resignDate, ValidationContext context)
        {
            var resignation = (Resignation)context.ObjectInstance;
            if (resignDate < resignation.NoticeDate)
            {
                return new ValidationResult("Resignation date cannot be earlier than notice date.");
            }
            return ValidationResult.Success;
        }
    }
}
