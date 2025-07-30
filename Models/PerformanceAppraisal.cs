using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class PerformanceAppraisal
    {
        [Key]
        public int PerformanceAppriasalId { get; set; }

        [Required(ErrorMessage = "Appraisal Date is required.")]
        public DateTime AppraisalDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Customer Experience rating is required.")]
        public string CustomerExperience { get; set; }

        [Required(ErrorMessage = "Marketing rating is required.")]
        public string Marketing { get; set; }

        [Required(ErrorMessage = "Management rating is required.")]
        public string Management { get; set; }

        [Required(ErrorMessage = "Administration rating is required.")]
        public string Administration { get; set; }

        [Required(ErrorMessage = "Presentation Skills rating is required.")]
        public string PresentationSkills { get; set; }

        [Required(ErrorMessage = "Quality of Work rating is required.")]
        public string QualityofWork { get; set; }

        [Required(ErrorMessage = "Efficiency rating is required.")]
        public string Efficiency { get; set; }

        [Required(ErrorMessage = "Integrity rating is required.")]
        public string Integrity { get; set; }

        [Required(ErrorMessage = "Professionalism rating is required.")]
        public string Professionalism { get; set; }

        [Required(ErrorMessage = "Team Work rating is required.")]
        public string TeamWork { get; set; }

        [Required(ErrorMessage = "Critical Thinking rating is required.")]
        public string CriticalThinking { get; set; }

        [Required(ErrorMessage = "Conflict Management rating is required.")]
        public string ConflictManagement { get; set; }

        [Required(ErrorMessage = "Attendance rating is required.")]
        public string Attendance { get; set; }

        [Required(ErrorMessage = "Ability to Meet Deadline rating is required.")]
        public string AbilityToMeetDeadline { get; set; }

        [ForeignKey("User")]
        [Required(ErrorMessage = "User selection is required.")]
        public int? UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Department")]
        [Required(ErrorMessage = "Department selection is required.")]
        public int? DepartmentId { get; set; }
        public Department Departments { get; set; }

        [ForeignKey("Designations")]
        [Required(ErrorMessage = "Designation selection is required.")]
        public int DesignationId { get; set; }
        public Designation Designations { get; set; }
    }
}
