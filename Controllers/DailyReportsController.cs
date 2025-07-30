using iTextSharp.text.pdf;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using iTextSharp.text;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class DailyReportsController : Controller
    {
        private readonly ApplicationDbContext db;

        public DailyReportsController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {

            return View();
        }


        [HttpGet]
        public IActionResult DailyReport(string dateRange, string status, string sort)
        {
            var today = DateTime.Today;
            var query = db.Attendance
                .Include(a => a.User)
                .ThenInclude(a => a.Department)
                .AsQueryable();

            // ✅ Apply Date Range Filter (if provided)
            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.Split(" - ");
                if (dates.Length == 2)
                {
                    DateTime startDate, endDate;
                    if (DateTime.TryParse(dates[0], out startDate) && DateTime.TryParse(dates[1], out endDate))
                    {
                        query = query.Where(a => a.Date >= startDate && a.Date <= endDate);
                    }
                }
            }
            else
            {
                // Default to today's records
                query = query.Where(a => a.Date == today);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // ✅ Apply Sorting (if provided)
            switch (sort)
            {
                case "recent":
                    query = query.OrderByDescending(a => a.Date);
                    break;
                case "asc":
                    query = query.OrderBy(a => a.User.FirstName);
                    break;
                case "desc":
                    query = query.OrderByDescending(a => a.User.FirstName);
                    break;
                case "lastMonth":
                    var lastMonth = today.AddMonths(-1);
                    query = query.Where(a => a.Date.Month == lastMonth.Month);
                    break;
                case "last7days":
                    var last7Days = today.AddDays(-7);
                    query = query.Where(a => a.Date >= last7Days);
                    break;
            }

            var attendanceList = query.ToList();

            var tasks = db.Task.Include(t => t.Project).ToList();
            int CompletedTasks = tasks.Count(t => t.Status == "Completed");
            int InProgressTasks = tasks.Count(t => t.Status == "Inprogress");

            int totalTasks = tasks.Count();
            ViewBag.CompletedPercentage = totalTasks > 0 ? (CompletedTasks * 100 / totalTasks) : 0;
            ViewBag.InProgressPercentage = totalTasks > 0 ? (InProgressTasks * 100 / totalTasks) : 0;

            ViewBag.Completed = CompletedTasks;
            ViewBag.InProgress = InProgressTasks;


            int totalPresent = attendanceList.Count(a => a.Status == "Present");
            int totalAbsent = attendanceList.Count(a => a.Status == "Absent");

            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalAbsent = totalAbsent;

            return View(attendanceList);
        }


        [HttpGet]
        public IActionResult ExportToCsv(string filterType)
        {
            // Fetch attendance data including User and Department
            var attendanceRecords = db.Attendance
                .Include(a => a.User)
                .ThenInclude(u => u.Department) // Include Department Table
                .AsQueryable();

            if (filterType == "weekly")
            {
                var startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);
                attendanceRecords = attendanceRecords.Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek);
            }
            else if (filterType == "monthly")
            {
                attendanceRecords = attendanceRecords.Where(a => a.Date.Month == DateTime.Now.Month);
            }

            // Create CSV string with updated structure
            var csv = "Name,Date,Department,Status\n"; // Header Row
            foreach (var record in attendanceRecords)
            {
                string fullName = $"{record.User.FirstName} {record.User.LastName}";
                string departmentName = record.User.Department != null ? record.User.Department.Name : "N/A";

                csv += $"{fullName},{record.Date.ToShortDateString()},{departmentName},{record.Status}\n";
            }

            // Return the CSV file
            var bytes = Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "daily_report.csv");
        }


        [HttpGet]
        public IActionResult ExportToPdf(string filterType)
        {
            var attendanceRecords = db.Attendance
                .Include(a => a.User)
                .ThenInclude(u => u.Department)
                .AsQueryable();

            if (filterType == "weekly")
            {
                var startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);
                attendanceRecords = attendanceRecords.Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek);
            }
            else if (filterType == "monthly")
            {
                attendanceRecords = attendanceRecords.Where(a => a.Date.Month == DateTime.Now.Month);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 10, 10, 20, 20);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add Title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                Paragraph title = new Paragraph("Daily Attendance Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(title);

                document.Add(new Paragraph(" "));

                // Create Table
                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 3, 2, 3, 2 });

                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                BaseColor headerColor = new BaseColor(52, 152, 219);

                PdfPCell[] headers = new PdfPCell[]
                {
            new PdfPCell(new Phrase("Name", headerFont)) { BackgroundColor = headerColor },
            new PdfPCell(new Phrase("Date", headerFont)) { BackgroundColor = headerColor },
            new PdfPCell(new Phrase("Department", headerFont)) { BackgroundColor = headerColor },
            new PdfPCell(new Phrase("Status", headerFont)) { BackgroundColor = headerColor }
                };

                foreach (var header in headers)
                {
                    header.HorizontalAlignment = Element.ALIGN_CENTER;
                    header.Padding = 5;
                    table.AddCell(header);
                }

                Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                foreach (var record in attendanceRecords)
                {
                    string fullName = $"{record.User.FirstName} {record.User.LastName}";
                    string departmentName = record.User.Department != null ? record.User.Department.Name : "N/A";

                    table.AddCell(new PdfPCell(new Phrase(fullName, rowFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(record.Date.ToShortDateString(), rowFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(departmentName, rowFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(record.Status, rowFont)) { Padding = 5 });
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "daily_report.pdf");
            }

            return BadRequest("Could not generate PDF.");
        }

        [HttpGet]
        public JsonResult GetAttendanceData(int year)
        {
            var attendanceData = db.Attendance
                .Include(a => a.User) // Include user details for searching
                .Where(a => a.Date.Year == year);


            var groupedData = attendanceData
                .GroupBy(a => a.Date.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Present = g.Count(a => a.Status == "Present"),
                    Absent = g.Count(a => a.Status == "Absent")
                })
                .OrderBy(a => a.Month)
                .ToList();

            var presentList = groupedData.Select(a => a.Present).ToList();
            var absentList = groupedData.Select(a => a.Absent).ToList();

            return Json(new { present = presentList, absent = absentList });
        }
        public async Task<IActionResult> Search(string name, string department)
        {
            // Debug output to verify search parameters
            Console.WriteLine($"Search Name: {name}, Department: {department}");

            var users = db.User.AsQueryable();

            // Search by name (FirstName or LastName) if a value is provided
            if (!string.IsNullOrEmpty(name))
            {
                users = users.Where(u => u.FirstName.Contains(name) || u.LastName.Contains(name));
            }

            // Search by department if a value is provided
            if (!string.IsNullOrEmpty(department))
            {
                users = users.Where(u => u.Department.Name.Contains(department)); // Assuming Department is a navigation property
            }

            var result = await users.ToListAsync();
            return Json(result);  // Return as JSON for AJAX processing
        }

        public async Task<IActionResult> GetUsers()
        {
            var users = await db.User.Include(u => u.Department).ToListAsync();  // Get all users with their department info
            return Json(users);  // Return all users as JSON for the table
        }
    }
}



