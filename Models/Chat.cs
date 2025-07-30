using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pulse360.Models
{
	public class Chat
	{
		[Key]
		public int ChatID { get; set; }

		[Required]
		public int SenderID { get; set; }

		[Required]
		public int ReceiverID { get; set; } // For private messages; consider GroupID for group chats.

		public string Message { get; set; } // Can be null for file sharing

		public DateTime Timestamp { get; set; } = DateTime.Now;

		[Required]
		public MessageStatus Status { get; set; } = MessageStatus.Sent; // Track message status

		public bool IsDeleted { get; set; } = false; // Soft delete flag

		[ForeignKey("SenderID")]
		public virtual User Sender { get; set; }

		[ForeignKey("ReceiverID")]
		public virtual User Receiver { get; set; }
	}

	public enum MessageStatus
	{
		Sent,
		Delivered,
		Read
	}
}

