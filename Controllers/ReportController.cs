using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using System.Text;

namespace Pulse360.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext db;
        public ReportController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EmployeeReport()
        {
            // Fetch data for cards
            ViewBag.TotalUsers = db.User.Count();
            ViewBag.ActiveUsers = db.User.Count(u => u.Status == "Active");
            ViewBag.InactiveUsers = db.User.Count(u => u.Status == "Deactive");
            ViewBag.RolesCount = db.Role.Count();

            // Prepare data for bar graph
            var userMonthGroups = db.User
            .GroupBy(u => new { u.DateOfJoining.Year, u.DateOfJoining.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                ActiveCount = g.Count(u => u.Status == "Active"),
                InactiveCount = g.Count(u => u.Status == "Deactive")
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToList();

            ViewBag.ChartLabels = userMonthGroups.Select(g => $"{g.Year}-{g.Month:00}").ToArray();
            ViewBag.ActiveData = userMonthGroups.Select(g => g.ActiveCount).ToArray();
            ViewBag.InactiveData = userMonthGroups.Select(g => g.InactiveCount).ToArray();

            //// Load Departments data from the database
            var departments = db.Departments
                .Select(d => new { d.DepartmentId, d.Name })
                .ToList();

            // Pass the data to ViewBag
            ViewBag.Departments = departments;

            return View();
        }

        public IActionResult ExportToCSV()
        {
            var users = db.User
            .Select(u => new
            {
                u.UserId,
                Name = $"{u.FirstName} {u.LastName}",
                u.Email,
                Department = u.Department.Name,
                u.PhoneNumber,
                DateOfJoining = u.DateOfJoining.ToString("yyyy-MM-dd"),
                u.Status
            })
            .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("UserId,Name,Email,Department,Contact Number,Date of Joining,Status");

            foreach (var user in users)
            {
                builder.AppendLine($"{user.UserId},{user.Name},{user.Email},{user.Department},{user.PhoneNumber},{user.DateOfJoining},{user.Status}");
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv", "EmployeeReport.csv");
        }

        public IActionResult ExportToPDF()
        {
            var users = db.User
            .Select(u => new
            {
                u.UserId,
                Name = $"{u.FirstName} {u.LastName}",
                u.Email,
                Department = u.Department.Name,
                u.PhoneNumber,
                DateOfJoining = u.DateOfJoining.ToString("yyyy-MM-dd"),
                u.Status
            })
            .ToList();

            using (var stream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, stream).CloseStream = false;
                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("Employee Report", titleFont) { Alignment = Element.ALIGN_CENTER };
                document.Add(title);
                document.Add(new Paragraph(" ")); // Blank line

                // Table
                var table = new PdfPTable(6) { WidthPercentage = 100 };
                table.AddCell("UserId");
                table.AddCell("Name");
                table.AddCell("Email");
                table.AddCell("Department");
                table.AddCell("Phone Number");
                table.AddCell("Date of Joining");

                foreach (var user in users)
                {
                    table.AddCell(user.UserId.ToString());
                    table.AddCell(user.Name);
                    table.AddCell(user.Email);
                    table.AddCell(user.Department);
                    table.AddCell(user.PhoneNumber);
                    table.AddCell(user.DateOfJoining);
                }

                document.Add(table);
                document.Close();

                var bytes = stream.ToArray();
                return File(bytes, "application/pdf", "EmployeeReport.pdf");
            }
        }

    }
}
