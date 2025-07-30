namespace Pulse360.Models
{
	public class ChatViewModel
	{
		public List<User> Users { get; set; }
		public User CurrentUser { get; set; }
		public User SelectedContact { get; set; }
		public List<Chat> Messages { get; set; }
	}
}
