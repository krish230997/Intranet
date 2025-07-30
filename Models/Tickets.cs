using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class Tickets
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [StringLength(200)]
        public string TicketTitle { get; set; }

        [Required]
        public string EventCategory { get; set; } // Foreign Key to EventCategory

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        public int AssignedBy { get; set; } // Foreign Key to User for AssignedBy

        [Required]
        public int AssignedTo { get; set; } // Foreign Key to User for AssignedTo

        public string TicketDescription { get; set; }

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } // Priority can be like Low, Medium, High

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Status can be Open, Closed, Pending, etc.

        [Required]
        [StringLength(10)]
        public string Visibility { get; set; } // Values: Public or Private

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual User AssignedByUser { get; set; }
        public virtual User AssignedToUser { get; set; }
    }
}
