using ClosedXML.Excel;
using iTextSharp.text.pdf;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;

namespace Pulse360.Controllers
{
    public class AdminAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AdminAttendanceList(string searchString, string status, string department, string sortOrder, DateTime? startDate, DateTime? endDate, int? page)
        {
            if (User.Identity.IsAuthenticated)
            {
                var role = User.FindFirstValue(ClaimTypes.Role);

                if (role != "Admin")
                {
                    TempData["error"] = "You do not have permission to view the attendance list.";
                    return RedirectToAction("AdminAttendanceList", "AdminAttendance");
                }
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }

            ViewData["SearchString"] = searchString;
            ViewData["StatusFilter"] = status;
            ViewData["DepartmentFilter"] = department;
            ViewData["SortOrder"] = sortOrder;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;

            var today = DateTime.Today;



            var data = _context.Attendance
    .Include(x => x.User)
        .ThenInclude(u => u.Department)
    .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(x => (x.User.FirstName + " " + x.User.LastName).Contains(searchString) ||
                                       (x.Status != null && x.Status.Contains(searchString)));

            }

            if (!string.IsNullOrEmpty(status))
            {
                data = data.Where(x => x.Status == status);
            }

            if (!string.IsNullOrEmpty(department))
            {
                data = data.Where(x => x.User.Department.Name == department);
            }

            if (startDate.HasValue)
            {
                data = data.Where(x => x.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                data = data.Where(x => x.Date <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(sortOrder))
            {
                switch (sortOrder)
                {
                    case "asc":
                        data = data.OrderBy(x => x.Date);
                        break;
                    case "desc":
                        data = data.OrderByDescending(x => x.Date);
                        break;
                    case "recent":
                        data = data.OrderByDescending(x => x.Date);
                        break;
                    case "lastMonth":
                        var lastMonth = today.AddMonths(-1);
                        data = data.Where(x => x.Date >= lastMonth).OrderByDescending(x => x.Date);
                        break;
                    case "last7Days":
                        var last7Days = today.AddDays(-7);
                        data = data.Where(x => x.Date >= last7Days).OrderByDescending(x => x.Date);
                        break;
                    default:
                        break;
                }
            }


            int pageSize = 10; // Default page size
            int pageIndex = page ?? 1; // Default to page 1 if no page is provided
            var paginatedData = data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            int totalEmployees = _context.User.Count(u => u.Role.RoleName == "Employee");




            //Uninformed and permission

            var approvedLeaveRequests = _context.LeaveRequests
        .Where(l => l.Status == "Approved" && l.StartDate <= today && l.EndDate >= today)
        .Select(l => l.UserId)
        .ToList();

            var absentUsers = data
       .Where(x => x.Status == "Absent" && x.Date == today)
       .Select(x => x.UserId)
       .ToList();


            int uninformed = absentUsers.Count(absentUserId => !approvedLeaveRequests.Contains(absentUserId));

            int permission = absentUsers.Count(absentUserId => approvedLeaveRequests.Contains(absentUserId));


            //absent employees for today

            var absentEmployees = data.Where(x => x.Status == "Absent" && x.Date == today)
                                      .Select(x => new
                                      {
                                          Id = x.User.UserId,
                                          FullName = x.User.FirstName + " " + x.User.LastName,
                                          ProfilePicture = x.User.ProfilePicture
                                      })
                                      .ToList();


            var present = data.Count(x => x.Status == "Present" && x.Date == today);
            var lateLogin = data.Count(x => x.Late > 0 && x.Date == today);
            var absent = data.Count(x => x.Status == "Absent" && x.Date == today);


            var departments = _context.Departments
        //.Where(d => d.Status == "Active") // Optional: filter active departments
        .Select(d => new { d.DepartmentId, d.Name })
        .ToList();

            // Pass departments to the view


            // Set ViewBag values
            ViewBag.Present = present;
            ViewBag.LateLogin = lateLogin;
            ViewBag.Absent = absent;
            ViewBag.Uninformed = uninformed;
            ViewBag.Permission = permission;
            ViewBag.TotalEmployees = totalEmployees;
            ViewBag.AbsentEmployees = absentEmployees;
            ViewBag.Departments = departments;

            return View(paginatedData);

        }


        public IActionResult EditAttendance(int AttendanceId)
        {
            var attendance = _context.Attendance.Find(AttendanceId);

            return View(attendance);

        }

        [HttpPost]
        public IActionResult EditAttendance(Attendance a)
        {

            var existingAttendance = _context.Attendance.Find(a.AttendanceId);

            existingAttendance.AttendanceId = a.AttendanceId;
            existingAttendance.Date = a.Date;
            existingAttendance.CheckIn = a.CheckIn;
            existingAttendance.CheckOut = a.CheckOut;
            existingAttendance.BreakHours = a.BreakHours;
            existingAttendance.Late = a.Late;
            existingAttendance.ProductionHours = a.ProductionHours;

            existingAttendance.Status = a.Status;

            _context.SaveChanges();
            return RedirectToAction("AdminAttendanceList");
        }


        public IActionResult ExportToPDF()
        {
            // Fetch data to export
            var data = _context.Attendance
                .Include(x => x.User)
                .Where(x => x.Date == DateTime.Today)
                .Select(x => new
                {
                    x.User.FirstName,
                    x.User.LastName,
                    x.Status,
                    x.CheckIn,
                    x.CheckOut,
                    x.BreakHours,
                    x.Late,
                    x.ProductionHours
                })
                .ToList();

            // Create PDF document
            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add content to the PDF
                Paragraph header = new Paragraph("Attendance Report - " + DateTime.Today.ToShortDateString())
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(header);

                PdfPTable table = new PdfPTable(8);
                table.AddCell("First Name");
                table.AddCell("Last Name");
                table.AddCell("Status");
                table.AddCell("Check-In");
                table.AddCell("Check-Out");
                table.AddCell("Break");
                table.AddCell("Late");
                table.AddCell("Production Hours");

                foreach (var item in data)
                {
                    table.AddCell(item.FirstName);
                    table.AddCell(item.LastName);
                    table.AddCell(item.Status);
                    table.AddCell(item.CheckIn?.ToString("HH:mm:ss"));
                    table.AddCell(item.CheckOut?.ToString("HH:mm:ss"));
                    table.AddCell(item.BreakHours.ToString());
                    table.AddCell(item.Late.ToString());
                    table.AddCell(item.ProductionHours.ToString());
                }

                document.Add(table);
                document.Close();

                // Return the PDF as a file
                return File(stream.ToArray(), "application/pdf", "AttendanceReport.pdf");
            }
        }


        public IActionResult ExportToExcel()
        {
            // Fetch data to export
            var data = _context.Attendance
                .Include(x => x.User)
                .Where(x => x.Date == DateTime.Today)
                .Select(x => new
                {
                    x.User.FirstName,
                    x.User.LastName,
                    x.Status,
                    x.CheckIn,
                    x.CheckOut,
                    x.BreakHours,
                    x.Late,
                    x.ProductionHours

                })
                .ToList();

            // Create Excel workbook
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Attendance");
                worksheet.Cell(1, 1).Value = "First Name";
                worksheet.Cell(1, 2).Value = "Last Name";
                worksheet.Cell(1, 3).Value = "Status";
                worksheet.Cell(1, 4).Value = "Check-In";
                worksheet.Cell(1, 5).Value = "Check-Out";
                worksheet.Cell(1, 6).Value = "Break Hours";
                worksheet.Cell(1, 7).Value = "Late";
                worksheet.Cell(1, 8).Value = "Production Hours";

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.FirstName;
                    worksheet.Cell(row, 2).Value = item.LastName;
                    worksheet.Cell(row, 3).Value = item.Status;
                    worksheet.Cell(row, 4).Value = item.CheckIn?.ToString("HH:mm:ss");
                    worksheet.Cell(row, 5).Value = item.CheckOut?.ToString("HH:mm:ss");
                    worksheet.Cell(row, 6).Value = item.BreakHours;
                    worksheet.Cell(row, 7).Value = item.Late;
                    worksheet.Cell(row, 8).Value = item.ProductionHours;
                    row++;
                }

                // Save the Excel file to a memory stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    // Return the Excel file
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
                }
            }
        }

    }
}
