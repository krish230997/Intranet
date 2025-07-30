using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Microsoft.EntityFrameworkCore;

namespace Pulse360.Controllers
{
    public class EmpReportController : Controller
    {
        private readonly ApplicationDbContext db;
        public EmpReportController(ApplicationDbContext db)
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
            ViewBag.InactiveUsers = db.User.Count(u => u.Status == "Inactive");
            ViewBag.RolesCount = db.Role.Count();
            ViewBag.DepartmentCount = db.Departments.Count();
            // Prepare data for bar graph
            var UsersMonthGroups = db.User
            .Where(u => u.Role.RoleName != "Admin")
            .GroupBy(u => new { u.DateOfJoining.Year, u.DateOfJoining.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                ActiveCount = g.Count(u => u.Status == "Active"),
                InactiveCount = g.Count(u => u.Status == "Inactive")
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToList();

            ViewBag.ChartLabels = UsersMonthGroups.Select(g => $"{g.Year}-{g.Month:00}").ToArray();
            ViewBag.ActiveData = UsersMonthGroups.Select(g => g.ActiveCount).ToArray();
            ViewBag.InactiveData = UsersMonthGroups.Select(g => g.InactiveCount).ToArray();

            //// Load Departments data from the database
            var departments = db.Departments
                .Select(d => new { d.DepartmentId, d.Name })
                .ToList();

            // Pass the data to ViewBag

            if(departments!=null)
            {
                ViewBag.Departments = departments;
            }
            

            var usersList = db.User
        .Include(u => u.Department)
       .Where(u => u.Role.RoleName != "Admin")
       .ToList();

            return View(usersList);
        }

        public IActionResult EmpExportToCSV()
        {
            var Userss = db.User
            .Select(u => new
            {
                u.UserId,
                Name = $"{u.FirstName} {u.LastName}",
                //u.Username,
                u.ProfilePicture,
                u.Email,
                Department = u.Department.Name,
                u.PhoneNumber,
                DateOfJoining = u.DateOfJoining.ToString("yyyy-MM-dd"),
                u.Status
            })
            .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("UsersId,Name,Email,Department,Contact Number,Date of Joining,Status");

            foreach (var Users in Userss)
            {
                builder.AppendLine($"{Users.UserId},{Users.Name},{Users.Email},{Users.Department},{Users.DateOfJoining},{Users.Status}");
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv", "EmployeeReport.csv");
        }

        public IActionResult EmpExportToPDF()
        {
            var Userss = db.User
            .Select(u => new
            {
                u.UserId,
                Name = $"{u.FirstName} {u.LastName}",
                //u.Username,
                u.ProfilePicture,
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
                table.AddCell("UsersId");
                table.AddCell("Name");
                table.AddCell("Email");
                table.AddCell("Department");
                table.AddCell("Phone Number");
                table.AddCell("Date of Joining");

                foreach (var Users in Userss)
                {
                    table.AddCell(Users.UserId.ToString());
                    table.AddCell(Users.Name);
                    table.AddCell(Users.Email);
                    table.AddCell(Users.Department);
                    table.AddCell(Users.PhoneNumber);
                    table.AddCell(Users.DateOfJoining);
                }

                document.Add(table);
                document.Close();

                var bytes = stream.ToArray();
                return File(bytes, "application/pdf", "EmployeeReport.pdf");
            }
        }

        public IActionResult FilteredAttendanceAction(string searchString, string status, int rowsPerPage = 10, int page = 1, string sortBy = "recent")
        {

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var query = db.User.Where(x => x.UserId == int.Parse(userId));
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();

                    query = query.Where(x =>
                        (x.Status != null && x.Status.ToLower().Contains(searchString)) ||
                        (x.DateOfJoining != null && x.DateOfJoining.ToString("yyyy-MM-dd").Contains(searchString)) ||
                         (x.Email != null && x.Email.ToLower().Contains(searchString)) ||
                          (x.FirstName != null && x.FirstName.ToLower().Contains(searchString)) ||
                           (x.Department != null && x.Department.Name.ToLower().Contains(searchString))
                         );
                    if (!string.IsNullOrEmpty(status) && status != "All")
                    {
                        query = query.Where(x => x.Status.ToLower() == status.ToLower());
                    }
                    switch (sortBy)
                    {
                        case "asc":
                            query = query.OrderBy(x => x.DateOfJoining);
                            break;
                        case "desc":
                            query = query.OrderByDescending(x => x.DateOfJoining); // Adjust column as needed
                            break;
                        case "recent":
                            query = query.OrderByDescending(x => x.DateOfJoining); // Most recent first
                            break;
                        case "last7Days":
                            query = query.Where(x => x.DateOfJoining >= DateTime.Now.AddDays(-7)).OrderByDescending(x => x.DateOfJoining);
                            break;
                        case "lastMonth":
                            query = query.Where(x => x.DateOfJoining >= DateTime.Now.AddMonths(-1)).OrderByDescending(x => x.DateOfJoining);
                            break;
                        default:
                            query = query.OrderByDescending(x => x.DateOfJoining); // Default sorting
                            break;
                    }
                    int totalRecords = query.Count();
                    var attendanceList = query.Skip((page - 1) * rowsPerPage).Take(rowsPerPage).ToList();

                    ViewData["SearchString"] = searchString;
                    ViewData["Status"] = status;
                    ViewData["RowsPerPage"] = rowsPerPage;
                    ViewData["TotalRecords"] = totalRecords;
                    ViewData["CurrentPage"] = page;
                    ViewData["SortBy"] = sortBy;
                    return Json(attendanceList);
                }

            }

            return View();
        }

        public IActionResult ProjectReport()
        {
            return View();
        }
        public IActionResult GetEmployees(int page = 1, string search = "", string status = "All")
        {
            int pageSize = 10;
            var query = db.User.AsQueryable();

            // Filter by search input
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.FirstName.Contains(search) || e.Email.Contains(search));
            }

            // Filter by status
            if (status != "All")
            {
                query = query.Where(e => e.Status == status);
            }

            // Pagination logic
            int totalRecords = query.Count();
            var employees = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Json(new { employees, totalPages = (int)Math.Ceiling((double)totalRecords / pageSize), currentPage = page });
        }

    }
}
