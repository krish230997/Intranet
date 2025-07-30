using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse360.Models
{
    public class Training
    {
        [Key]
        public int TrainingId { get; set; }

        [ForeignKey("Trainer")]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }  // Navigation property for Trainer

        // Foreign key to TrainingType table
        [ForeignKey("TrainingType")]
        public int TrainingTypeId { get; set; }
        public TrainingType TrainingType { get; set; }  // Navigation property for TrainingType

        // Foreign key to User table (Employee)
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }  // Navigation property for User (Employee)

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TrainingCost { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // New columns added
        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public string ModifiedBy { get; set; }

        [Required]
        public DateTime ModifiedAt { get; set; }

        [NotMapped]
        public string? ProfilePicture { get; set; }

        [NotMapped]
        public List<string> images { get; set; } = new List<string>();
    }
}
