using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
namespace Pulse360.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch current logged-in user
            string em = HttpContext.Session.GetString("User"); // Assuming session contains User Email
            var currentUser = await _context.User.FirstOrDefaultAsync(x => x.Email == em);

            if (currentUser != null)
            {
                ViewBag.CurrentUserId = currentUser.UserId; // Pass current user ID to the view
                ViewBag.ProfilePicture = currentUser.ProfilePicture; //fetch profile pic 
            }

            // Fetch counts for the summary boxes
            ViewBag.OpenTicketsCount = await _context.Tickets.CountAsync(t => t.Status != "Closed");
            ViewBag.InProgressTicketsCount = await _context.Tickets.CountAsync(t => t.Status == "In Progress");
            ViewBag.HighPriorityTicketsCount = await _context.Tickets.CountAsync(t => t.Priority == "High");
            ViewBag.LowPriorityTicketsCount = await _context.Tickets.CountAsync(t => t.Priority == "Low");

            // Fetch tickets with related user details, excluding "Closed" status
            var tickets = await _context.Tickets
                .Where(t => t.Status != "Closed")
                .Include(t => t.AssignedByUser)
                .Include(t => t.AssignedToUser)
                .ToListAsync();

            // Fetch users for the dropdown
            ViewBag.Users = await _context.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToListAsync();

            // Fetch categories for the dropdown
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // Fetch ticket counts per category
            var ticketCountsPerCategory = await _context.Tickets
                .GroupBy(t => t.EventCategory)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Category, g => g.Count);

            ViewBag.TicketCountsPerCategory = ticketCountsPerCategory;

            return View(tickets);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName))
            {
                var category = new Category { CategoryName = categoryName };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        public IActionResult Create(Tickets ticket)
        {
            string em = HttpContext.Session.GetString("User"); // Assuming session contains UserId
            var data = _context.User.Where(x => x.Email.Equals(em)).SingleOrDefault();
            ticket.AssignedBy = data.UserId;
            ticket.Visibility = "Public"; // Default visibility
            ticket.CreatedAt = DateTime.Now;

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var tickets = _context.Tickets
                .Include(t => t.AssignedByUser)
                .Include(t => t.AssignedToUser)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Tickets");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Ticket ID";
                worksheet.Cell(currentRow, 2).Value = "Title";
                worksheet.Cell(currentRow, 3).Value = "Category";
                worksheet.Cell(currentRow, 4).Value = "Priority";
                worksheet.Cell(currentRow, 5).Value = "Status";
                worksheet.Cell(currentRow, 6).Value = "Assigned To";
                worksheet.Cell(currentRow, 7).Value = "Assigned By";
                worksheet.Cell(currentRow, 8).Value = "Created At";

                foreach (var ticket in tickets)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = ticket.TicketId;
                    worksheet.Cell(currentRow, 2).Value = ticket.TicketTitle;
                    worksheet.Cell(currentRow, 3).Value = ticket.EventCategory;
                    worksheet.Cell(currentRow, 4).Value = ticket.Priority;
                    worksheet.Cell(currentRow, 5).Value = ticket.Status;
                    worksheet.Cell(currentRow, 6).Value = ticket.AssignedToUser?.FirstName + " " + ticket.AssignedToUser?.LastName;
                    worksheet.Cell(currentRow, 7).Value = ticket.AssignedByUser?.FirstName + " " + ticket.AssignedByUser?.LastName;
                    worksheet.Cell(currentRow, 8).Value = ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
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
            var tickets = _context.Tickets
                .Include(t => t.AssignedByUser)
                .Include(t => t.AssignedToUser)
                .ToList();

            using (var stream = new MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Paragraph title = new Paragraph("Tickets Report", titleFont) { Alignment = Element.ALIGN_CENTER };
                pdfDoc.Add(title);
                pdfDoc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                float[] columnWidths = { 10f, 20f, 15f, 10f, 10f, 15f, 15f, 20f };
                table.SetWidths(columnWidths);

                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                string[] headers = { "Ticket ID", "Title", "Category", "Priority", "Status", "Assigned To", "Assigned By", "Created At" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var ticket in tickets)
                {
                    table.AddCell(new Phrase(ticket.TicketId.ToString(), rowFont));
                    table.AddCell(new Phrase(ticket.TicketTitle, rowFont));
                    table.AddCell(new Phrase(ticket.EventCategory, rowFont));
                    table.AddCell(new Phrase(ticket.Priority, rowFont));
                    table.AddCell(new Phrase(ticket.Status, rowFont));
                    table.AddCell(new Phrase(ticket.AssignedToUser?.FirstName + " " + ticket.AssignedToUser?.LastName, rowFont));
                    table.AddCell(new Phrase(ticket.AssignedByUser?.FirstName + " " + ticket.AssignedByUser?.LastName, rowFont));
                    table.AddCell(new Phrase(ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), rowFont));
                }

                pdfDoc.Add(table);
                pdfDoc.Close();

                return File(stream.ToArray(), "application/pdf", "Tickets.pdf");
            }
        }

    }
}