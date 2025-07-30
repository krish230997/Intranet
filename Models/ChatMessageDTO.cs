namespace Pulse360.Models
{
	public class ChatMessageDTO
	{
		public int SenderId { get; set; }
		public int ReceiverId { get; set; }
		public string Message { get; set; }
	}
}
