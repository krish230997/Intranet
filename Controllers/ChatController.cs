using ChatApplication.Hubs;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.Security.Claims;

namespace Pulse360.Controllers
{
	public class ChatController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ChatController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int? contactId)
		{
			var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
			var users = await _context.User.ToListAsync();

			//if (userId == null || !users.Any(u => u.UserId == userId))
			//{
			//	return RedirectToAction("SelectUser");
			//}

			var selectedUser = users.FirstOrDefault(u => u.UserId == userId);
			var selectedContact = users.FirstOrDefault(u => u.UserId == contactId);

			// Retrieve messages excluding deleted ones
			var chatMessages = await _context.Chats
				.Where(c => ((c.SenderID == userId && c.ReceiverID == contactId) ||
							 (c.SenderID == contactId && c.ReceiverID == userId)) &&
							!c.IsDeleted) // Exclude soft-deleted messages
				.Include(c => c.Sender)
				.Include(c => c.Receiver)
				.OrderBy(c => c.Timestamp)
				.ToListAsync();

			var chatViewModel = new ChatViewModel
			{
				Users = users,
				CurrentUser = selectedUser,
				SelectedContact = selectedContact,
				Messages = chatMessages
			};

			return View(chatViewModel);
		}

		// ✅ Page for users to select themselves before chatting
		public async Task<IActionResult> SelectUser()
		{
			var users = await _context.User.ToListAsync();
			ViewBag.Users = users;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> SendMessage([FromBody] ChatMessageDTO chatMessage)
		{
			if (string.IsNullOrWhiteSpace(chatMessage.Message))
			{
				return BadRequest(new { error = "Message cannot be empty." });
			}

			var chat = new Chat
			{
				SenderID = chatMessage.SenderId,
				ReceiverID = chatMessage.ReceiverId,
				Message = chatMessage.Message,
				Timestamp = DateTime.Now,
				Status = MessageStatus.Sent,
				IsDeleted = false // Default to not deleted
			};

			_context.Chats.Add(chat);
			await _context.SaveChangesAsync();

			// Notify all clients via SignalR
			var hubContext = HttpContext.RequestServices.GetService(typeof(IHubContext<ChatHub>)) as IHubContext<ChatHub>;
			await hubContext.Clients.All.SendAsync("ReceiveMessage", chat.SenderID, chat.ReceiverID, chat.Message);

			return Ok(new { success = true });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteMessage(int chatId, int userId, int contactId)
		{
			var chat = await _context.Chats.FindAsync(chatId);
			if (chat != null)
			{
				chat.IsDeleted = true; // Soft delete the message
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Index", new { userId = userId, contactId = contactId });
		}


	}
}
