using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClosedXML.Excel;

namespace Pulse360.Controllers
{
    public class TimesheetController : Controller
    {
        private ApplicationDbContext db;
        public TimesheetController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [Authorize(Roles = "Employee")]
        public IActionResult Timesheet()
        {
            ViewBag.project = new SelectList(db.AllProjects, "ProjectId", "ProjectName");
            return View();
        }

        public IActionResult FetchTimesheet()
        {
            // Get the current user's username
            var username = User.Identity.Name;

            // Query the database for timesheets created by the logged-in user
            var data = db.Timesheets
                .Where(t => t.CreatedBy == username) // Filter for the current user's entries
                .Include(t => t.User) // Include the related User entity
                .Include(t => t.Projects) // Include the related Project entity
                .Select(t => new
                {
                    t.TimesheetId,
                    t.UserId,
                    User = new { t.User.FirstName, t.User.LastName, t.User.Email, t.User.ProfilePicture },
                    t.ProjectId,
                    Projects = new { t.Projects.ProjectName }, // Correct reference
                    t.Date,
                    t.WorkHours,
                    t.Status,
                    t.CreatedBy,
                    t.CreatedAt,
                    t.ApprovedBy,
                    t.ApprovedAt
                })
                .ToList();

            // Return the filtered data as JSON
            return Json(data);
        }


        [HttpPost]
        public IActionResult AddTimesheet(Timesheet timesh)
        {
            var username = User.Identity.Name;
            var currentUser = db.User.FirstOrDefault(u => u.Email == username);

            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            timesh.UserId = currentUser.UserId;
            timesh.Status = "Pending";
            timesh.CreatedBy = username;
            timesh.CreatedAt = DateTime.UtcNow;

            db.Timesheets.Add(timesh);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult Searching(string mydata)
        {
            // Get the current user's username
            var username = User.Identity.Name;

            // Filter timesheets by the logged-in user and search term
            var data = db.Timesheets
                .Where(t => t.CreatedBy == username) // Ensure the timesheets belong to the logged-in user
                .Include(t => t.User) // Include User entity
                .Include(t => t.Projects) // Include related Project entity
                .Where(t => string.IsNullOrEmpty(mydata) || t.Projects.ProjectName.Contains(mydata)) // Filter by ProjectName
                .Select(t => new
                {
                    t.TimesheetId,
                    Projects = new { t.Projects.ProjectName }, // Include ProjectName
                    t.Date,
                    t.WorkHours,
                    t.Status,
                    User = new { t.User.FirstName, t.User.LastName, t.User.Email, t.User.ProfilePicture } // Include User Email
                })
                .ToList();

            return Json(data);
        }

        public IActionResult Sorting(string mydata)
        {
            // Get the current user's username
            var username = User.Identity.Name;

            // Filter timesheets by the logged-in user
            var data = db.Timesheets
                .Where(t => t.CreatedBy == username)
                .Include(t => t.User)
                .Include(t => t.Projects)
                .Select(t => new
                {
                    t.TimesheetId,
                    Projects = new { t.Projects.ProjectName }, // Include ProjectName
                    t.Date,
                    t.WorkHours,
                    t.Status,
                    User = new { t.User.FirstName, t.User.LastName, t.User.Email, t.User.ProfilePicture } // Include User Email
                });

            // Apply sorting based on the provided parameter
            data = mydata == "asc" ? data.OrderBy(t => t.Date) : data.OrderByDescending(t => t.Date);

            return Json(data.ToList());
        }


        [HttpGet]
        public IActionResult FetchWeeklyTimesheets()
        {
            var username = User.Identity.Name;
            var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var data = db.Timesheets
                .Where(t => t.CreatedBy == username && t.Date >= startOfWeek && t.Date < endOfWeek)
                .Include(t => t.User)
                .Include(t => t.Projects) // Include the related Project entity
                .Select(t => new
                {
                    t.TimesheetId,
                    Projects = new { t.Projects.ProjectName }, // Select specific Project properties
                    t.Date,
                    t.WorkHours,
                    t.Status,
                    User = new { t.User.FirstName, t.User.LastName, t.User.Email, t.User.ProfilePicture }
                })
                .ToList();

            return Json(data);
        }

        [HttpGet]
        public IActionResult FetchMonthlyTimesheets()
        {
            var username = User.Identity.Name;
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var data = db.Timesheets
                .Where(t => t.CreatedBy == username && t.Date >= startOfMonth && t.Date <= endOfMonth)
                .Include(t => t.User)
                .Include(t => t.Projects)
                .Select(t => new
                {
                    t.TimesheetId,
                    Projects = new { t.Projects.ProjectName },
                    t.Date,
                    t.WorkHours,
                    t.Status,
                    User = new { t.User.FirstName, t.User.LastName, t.User.Email, t.User.ProfilePicture }
                })
                .ToList();

            return Json(data);
        }

        [HttpGet]
        public IActionResult FetchRecentlyAddedTimesheets()
        {
            var username = User.Identity.Name;

            var data = db.Timesheets
                .Where(t => t.CreatedBy == username)
                .Include(t => t.User)
                .Include(t => t.Projects)
                .OrderByDescending(t => t.Date) // Sort by recent first
                .Select(t => new
                {
                    t.TimesheetId,
                    Projects = new { t.Projects.ProjectName },
                    t.Date,
                    t.WorkHours,
                    t.Status,
                    User = new { t.User.FirstName, t.User.LastName, t.User.Email, t.User.ProfilePicture }
                })
                .ToList();

            return Json(data);
        }

     


        [HttpPost]
        public IActionResult SendForApproval(string timesheetIds)
        {
            if (string.IsNullOrEmpty(timesheetIds))
            {
                TempData["error"] = "No timesheets selected for approval.";
                return RedirectToAction("Timesheet");
            }

            var ids = timesheetIds.Split(',').Select(int.Parse).ToList();
            var email = User.Identity.Name;
            var timesheets = db.Timesheets
                .Where(t => ids.Contains(t.TimesheetId) && t.CreatedBy == email)
                .ToList();

            foreach (var timesheet in timesheets)
                timesheet.Status = "Pending Approval";

            db.SaveChanges();
            TempData["success"] = "Timesheets sent for approval.";
            return RedirectToAction("Timesheet");
        }

        public IActionResult ExportToPdf()
        {
            var timesheets = db.Timesheets
                .Include(t => t.User)
                .Include(t => t.Projects)
                .ToList();

            MemoryStream workStream = new MemoryStream();
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, workStream);
            document.Open();

            Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font fontContent = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable table = new PdfPTable(5); // Number of columns
            table.WidthPercentage = 100;

            // Table headers
            table.AddCell(new PdfPCell(new Phrase("Employee", fontHeader)));
            table.AddCell(new PdfPCell(new Phrase("Project", fontHeader)));
            table.AddCell(new PdfPCell(new Phrase("Date", fontHeader)));
            table.AddCell(new PdfPCell(new Phrase("Worked Hours", fontHeader)));
            table.AddCell(new PdfPCell(new Phrase("Status", fontHeader)));

            // Table content
            foreach (var t in timesheets)
            {
                table.AddCell(new PdfPCell(new Phrase($"{t.User.FirstName} {t.User.LastName}", fontContent)));
                table.AddCell(new PdfPCell(new Phrase(t.Projects.ProjectName, fontContent)));
                table.AddCell(new PdfPCell(new Phrase(t.Date.ToString("yyyy-MM-dd"), fontContent)));
                table.AddCell(new PdfPCell(new Phrase(t.WorkHours.ToString(), fontContent)));
                table.AddCell(new PdfPCell(new Phrase(t.Status, fontContent)));
            }

            document.Add(table);
            document.Close();

            byte[] byteArray = workStream.ToArray();
            return File(byteArray, "application/pdf", "Timesheets.pdf");
        }

        public IActionResult ExportToExcel()
        {
            var timesheets = db.Timesheets
                .Include(t => t.User)
                .Include(t => t.Projects)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Timesheets");

                // Add headers
                worksheet.Cell(1, 1).Value = "Employee";
                worksheet.Cell(1, 2).Value = "Project";
                worksheet.Cell(1, 3).Value = "Date";
                worksheet.Cell(1, 4).Value = "Worked Hours";
                worksheet.Cell(1, 5).Value = "Status";

                int row = 2; // Start from row 2 since row 1 has headers
                foreach (var t in timesheets)
                {
                    worksheet.Cell(row, 1).Value = $"{t.User.FirstName} {t.User.LastName}";
                    worksheet.Cell(row, 2).Value = t.Projects.ProjectName;
                    worksheet.Cell(row, 3).Value = t.Date.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = t.WorkHours;
                    worksheet.Cell(row, 5).Value = t.Status;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Timesheets.xlsx");
                }
            }
        }
    }
}
