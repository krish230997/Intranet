using ClosedXML.Excel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class TicketReplyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketReplyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tickets = _context.Tickets.Where(t => t.Visibility == "Public").ToList();
            var categories = _context.Categories.ToList(); // Fetch categories from the database

            var ticketsWithNames = tickets.Select(t => new
            {
                t.TicketId,
                t.EventCategory,
                t.TicketTitle,
                t.TicketDescription,
                t.Priority,
                t.Status,
                t.AssignedBy,
                AssignedToName = _context.User
                    .Where(u => u.UserId == t.AssignedTo)
                    .Select(u => u.FirstName)
                    .FirstOrDefault(),
                UpdatedAt = (DateTime.Now - t.CreatedAt).Hours,
                ReplyCount = _context.TicketReplies.Count(r => r.TicketId == t.TicketId)
            }).ToList();

            ViewBag.Tickets = ticketsWithNames;
            ViewBag.Categories = categories; // Pass categories to the view

            return View();
        }


        [HttpPost]
        public IActionResult AddReply(int ticketId, string replyMessage, string repliedBy)
        {
            var u = HttpContext.Session.GetString("UserN");
            
            //var currentUser = await _context.User.FirstOrDefaultAsync(x => x.Email == u);
            repliedBy = u;
            if (!string.IsNullOrEmpty(replyMessage) && ticketId > 0)
            {
                var reply = new TicketReplies
                {
                    TicketId = ticketId,
                    ReplyMessage = replyMessage,
                    RepliedBy = repliedBy,
                    RepliedAt = DateTime.Now
                };

                _context.TicketReplies.Add(reply);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult GetReplies(int ticketId)
        {
            // Fetch replies for a specific ticket
            var replies = _context.TicketReplies
                .Where(r => r.TicketId == ticketId)
                .OrderBy(r => r.RepliedAt)
                .ToList();

            return PartialView("_RepliesPartial", replies);
        }

        public static string GetPriorityBadgeClass(string priority)
        {
            return priority switch
            {
                "High" => "bg-danger text-white", // Red
                "Medium" => "bg-warning text-dark", // Orange
                "Low" => "bg-success text-white", // Green
                _ => "bg-secondary text-white" // Default
            };
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName,
                    pict = u.ProfilePicture
                })
                .ToList();

            var loggedInUserEmail = HttpContext.Session.GetString("User");
            var loggedInUser = _context.User.FirstOrDefault(u => u.Email == loggedInUserEmail);

            return Json(new { users, loggedInUser });
        }


        [HttpPost]
        public IActionResult AddTicket([FromBody] Tickets newTicket)
        {
            var loggedInUserEmail = HttpContext.Session.GetString("User");
            var loggedInUser = _context.User.FirstOrDefault(u => u.Email == loggedInUserEmail);

            if (newTicket == null || newTicket.AssignedTo == 0 || loggedInUser == null)
            {
                return BadRequest(new { message = "Invalid ticket data. Please select users." });
            }

            try
            {
                newTicket.Status = "Open";
                newTicket.CreatedAt = DateTime.Now;
                newTicket.AssignedBy = loggedInUser.UserId; // Set AssignedBy from session user

                _context.Tickets.Add(newTicket);
                _context.SaveChanges();

                return Ok(new { message = "Ticket added successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while saving the ticket.", error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var tickets = _context.Tickets.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Tickets");
                worksheet.Cell(1, 1).Value = "Ticket ID";
                worksheet.Cell(1, 2).Value = "Title";
                worksheet.Cell(1, 3).Value = "Description";
                worksheet.Cell(1, 4).Value = "Priority";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Created At";

                int row = 2;
                foreach (var ticket in tickets)
                {
                    worksheet.Cell(row, 1).Value = ticket.TicketId;
                    worksheet.Cell(row, 2).Value = ticket.TicketTitle;
                    worksheet.Cell(row, 3).Value = ticket.TicketDescription;
                    worksheet.Cell(row, 4).Value = ticket.Priority;
                    worksheet.Cell(row, 5).Value = ticket.Status;
                    worksheet.Cell(row, 6).Value = ticket.CreatedAt.ToString("yyyy-MM-dd");
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tickets.xlsx");
                }
            }
        }

        [HttpGet]
        public IActionResult ExportToPdf()
        {
            var tickets = _context.Tickets.ToList();

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 10, 10, 20, 20);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                Paragraph title = new Paragraph("Ticket Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(title);
                document.Add(new Paragraph(" "));

                // ❗ Fix: Updated table column count to match headers (4 columns)
                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 2, 4, 3, 2 }); // Adjusted column widths

                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                BaseColor headerColor = new BaseColor(52, 152, 219);

                string[] headers = { "ID", "Title", "Priority", "Status" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = headerColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                foreach (var ticket in tickets)
                {
                    table.AddCell(new PdfPCell(new Phrase(ticket.TicketId.ToString(), rowFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(ticket.TicketTitle, rowFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(ticket.Priority, rowFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(ticket.Status, rowFont)) { Padding = 5 });
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "Tickets.pdf");
            }
        }

    }
}