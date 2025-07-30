using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
    public class TicketReplies
    {
        [Key]
        public int ReplyId { get; set; }

        [Required]
        public int TicketId { get; set; } // Foreign Key referencing Tickets table

        [Required]
        [StringLength(500)]
        public string ReplyMessage { get; set; }

        [Required]
        [StringLength(100)]
        public string RepliedBy { get; set; } // Name/ID of the user adding the reply

        [Required]
        public DateTime RepliedAt { get; set; }

        // Navigation Property
        public virtual Tickets Ticket { get; set; }
    }
}
