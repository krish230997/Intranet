using System.Security.Claims;
using ClosedXML.Excel;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
namespace Pulse360.Controllers
{
    public class AttendanceReportController : Controller
    {
        private readonly ApplicationDbContext db;

        public AttendanceReportController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            // Fetch attendance data along with User details
            var attendanceWithUserData = db.Attendance
                .Include(a => a.User)  // Include User details in the result
                .Select(a => new
                {
                    a.AttendanceId,
                    a.UserId,
                    UserName = a.User.FirstName + " " + a.User.LastName,
                    a.User.ProfilePicture,
                    a.Date,
                    a.CheckIn,
                    a.CheckOut,
                    a.LunchIn,
                    a.LunchOut,
                    a.Status,
                    a.WorkingHours,
                    a.OvertimeHours,
                    a.BreakHours,
                    a.Late,
                    a.ProductionHours
                }).ToList();


            // Calculate total working hours, total half days, presents, and absents
            var monthlyData = attendanceWithUserData
         .GroupBy(a => new { a.Date.Month, a.Date.Year })
         .Select(g => new
         {
             Month = g.Key.Month,
             Year = g.Key.Year,
             PresentCount = g.Count(a => a.Status == "Present"),
             AbsentCount = g.Count(a => a.Status == "Absent")
         })
         .OrderBy(g => g.Year).ThenBy(g => g.Month)
         .ToList();

            // Prepare data for the graph
            var months = monthlyData.Select(d => $"{d.Month}-{d.Year}").ToList();
            var presentCounts = monthlyData.Select(d => d.PresentCount).ToList();
            var absentCounts = monthlyData.Select(d => d.AbsentCount).ToList();

            var totalRecords = attendanceWithUserData.Count();

            // Calculate total working hours, total half days, presents, and absents
            var totalWorkingHours = attendanceWithUserData.Sum(a => a.WorkingHours);
            var totalHalfDays = attendanceWithUserData.Count(a => a.Status == "Half Day");
            var totalPresent = attendanceWithUserData.Count(a => a.Status == "Present");
            var totalAbsent = attendanceWithUserData.Count(a => a.Status == "Absent");
            var totalLeaves = db.LeaveBalances.Count();
            var totalHoliday = db.Events.Count();

            var presentPercentage = totalRecords > 0 ? (totalPresent * 100) / totalRecords : 0;
            var halfDayPercentage = totalRecords > 0 ? (totalHalfDays * 100) / totalRecords : 0;
            var absentPercentage = totalRecords > 0 ? (totalAbsent * 100) / totalRecords : 0;
            var totalLeaveP = totalRecords > 0 ? (totalLeaves * 100) / totalRecords : 0;

            // Assuming maximum working hours in a month (Example: 160 hours)
            var maxWorkingHours = 160;
            var workingHoursPercentage = maxWorkingHours > 0 ? (totalWorkingHours * 100) / maxWorkingHours : 0;
            // Prepare a view model that includes the calculated totals and graph data
            var viewModel = new
            {
                TotalWorkingHours = totalWorkingHours,
                TotalHalfDays = totalHalfDays,
                TotalPresent = totalPresent,
                TotalAbsent = totalAbsent,
                AttendanceList = attendanceWithUserData,
                Months = months,
                PresentCounts = presentCounts,
                AbsentCounts = absentCounts,
                PresentPercentage = presentPercentage,
                HalfDayPercentage = halfDayPercentage,
                AbsentPercentage = absentPercentage,
                WorkingHoursPercentage = workingHoursPercentage,
                TotalLeave = totalLeaves,
                TotalLeaveP = totalLeaveP,
                TotalHoliday = totalHoliday

            };

            return View(viewModel);
        }

        public IActionResult ExportToPdf()
        {
            // Fetch attendance data
            var attendanceWithUserData = db.Attendance
                .Include(a => a.User)
                .Select(a => new
                {
                    a.AttendanceId,
                    a.UserId,
                    UserName = a.User.FirstName + " " + a.User.LastName,
                    a.Date,
                    a.CheckIn,
                    a.CheckOut,
                    a.LunchIn,
                    a.LunchOut,
                    a.Status,
                    a.WorkingHours,
                    a.OvertimeHours,
                    a.BreakHours,
                    a.Late,
                    a.ProductionHours
                }).ToList();

            // Create a new memory stream to hold the PDF
            using (var stream = new MemoryStream())
            {
                // Create a PdfWriter instance to write the document to the memory stream
                using (var writer = new PdfWriter(stream))
                {
                    // Create the PdfDocument instance
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf, PageSize.A4.Rotate());
                    document.SetMargins(10, 10, 10, 10);

                    // Add title to the document with a default font
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    document.Add(new Paragraph("Attendance Report")
                        .SetFont(font)
                        .SetFontSize(10)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20));



                    // Create a table to hold the data
                    var table = new Table(UnitValue.CreatePercentArray(13)); // 13 columns
                    table.UseAllAvailableWidth();
                    // Add header row
                    table.AddCell("AttendanceId");
                    table.AddCell("User Name");
                    table.AddCell("Date");
                    table.AddCell("Check-In");
                    table.AddCell("Check-Out");
                    table.AddCell("LunchIn");
                    table.AddCell("LunchOut");
                    table.AddCell("Status");
                    table.AddCell("WorkingHours");
                    table.AddCell("OvertimeHours");
                    table.AddCell("BreakHours");
                    table.AddCell("Late");
                    table.AddCell("ProductionHours");
                    // Add data rows
                    foreach (var item in attendanceWithUserData)
                    {
                        table.AddCell(item.AttendanceId.ToString());
                        table.AddCell(item.UserName);
                        table.AddCell(item.Date.ToString("yyyy-MM-dd"));
                        table.AddCell(item.CheckIn.ToString());
                        table.AddCell(item.CheckOut.ToString());
                        table.AddCell(item.LunchIn.ToString());
                        table.AddCell(item.LunchOut.ToString());
                        table.AddCell(item.Status);
                        table.AddCell(item.WorkingHours.ToString());
                        table.AddCell(item.OvertimeHours.ToString());
                        table.AddCell(item.BreakHours.ToString());
                        table.AddCell(item.Late.ToString());
                        table.AddCell(item.ProductionHours.ToString());
                    }

                    // Add table to the document
                    document.Add(table);

                    // Close the document
                    document.Close();
                }

                // Return the PDF as a downloadable file
                return File(stream.ToArray(), "application/pdf", "AttendanceReport.pdf");
            }
        }

        public IActionResult ExportToExcel()
        {
            // Fetch attendance data
            var attendanceWithUserData = db.Attendance
                .Include(a => a.User)
                .Select(a => new
                {
                    a.AttendanceId,
                    a.UserId,
                    UserName = a.User.FirstName + " " + a.User.LastName,
                    a.Date,
                    a.CheckIn,
                    a.CheckOut,
                    a.LunchIn,
                    a.LunchOut,
                    a.Status,
                    a.WorkingHours,
                    a.OvertimeHours,
                    a.BreakHours,
                    a.Late,
                    a.ProductionHours
                }).ToList();

            // Create a new Excel workbook
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet("Attendance Report");

                // Set headers
                worksheet.Cell(1, 1).Value = "AttendanceId";
                worksheet.Cell(1, 2).Value = "UserName";
                worksheet.Cell(1, 3).Value = "Date";
                worksheet.Cell(1, 4).Value = "CheckIn";
                worksheet.Cell(1, 5).Value = "CheckOut";
                worksheet.Cell(1, 6).Value = "LunchIn";
                worksheet.Cell(1, 7).Value = "LunchOut";
                worksheet.Cell(1, 8).Value = "Status";
                worksheet.Cell(1, 9).Value = "WorkingHours";
                worksheet.Cell(1, 10).Value = "OvertimeHours";
                worksheet.Cell(1, 11).Value = "BreakHours";
                worksheet.Cell(1, 12).Value = "Late";
                worksheet.Cell(1, 13).Value = "ProductionHours";

                // Add data rows
                int row = 2;
                foreach (var item in attendanceWithUserData)
                {
                    worksheet.Cell(row, 1).Value = item.AttendanceId;
                    worksheet.Cell(row, 2).Value = item.UserName;
                    worksheet.Cell(row, 3).Value = item.Date.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = item.CheckIn?.ToString("HH:mm");
                    worksheet.Cell(row, 5).Value = item.CheckOut?.ToString("HH:mm");
                    worksheet.Cell(row, 6).Value = item.LunchIn?.ToString("HH:mm");
                    worksheet.Cell(row, 7).Value = item.LunchOut?.ToString("HH:mm");
                    worksheet.Cell(row, 8).Value = item.Status;
                    worksheet.Cell(row, 10).Value = item.WorkingHours;
                    worksheet.Cell(row, 11).Value = item.OvertimeHours;
                    worksheet.Cell(row, 12).Value = item.BreakHours;
                    worksheet.Cell(row, 13).Value = item.Late;
                    worksheet.Cell(row, 14).Value = item.ProductionHours;
                    row++;
                }

                // Save the Excel file to a memory stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    // Return the Excel file as a downloadable file
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
                }
            }


        }

        [HttpGet]
        //public IActionResult GetFilteredReports(string dateFilter, string statusFilter, string sortOrder, DateTime? startDate, DateTime? endDate, string searchTerm)
        //{
        //    var attendanceData = db.Attendance.Include(a => a.User).AsQueryable();

        //    // Apply filters
        //    if (!string.IsNullOrEmpty(dateFilter))
        //    {
        //        DateTime today = DateTime.Today;
        //        switch (dateFilter.ToLower())
        //        {
        //            case "yesterday":
        //                attendanceData = attendanceData.Where(a => a.Date == today.AddDays(-1));
        //                break;
        //            case "last7days":
        //                attendanceData = attendanceData.Where(a => a.Date >= today.AddDays(-7));
        //                break;
        //            case "last30days":
        //                attendanceData = attendanceData.Where(a => a.Date >= today.AddDays(-30));
        //                break;
        //            case "lastyear":
        //                attendanceData = attendanceData.Where(a => a.Date.Year == today.Year - 1);
        //                break;
        //            case "thisyear":
        //                attendanceData = attendanceData.Where(a => a.Date.Year == today.Year);
        //                break;
        //            case "customrange":
        //                if (startDate.HasValue && endDate.HasValue)
        //                {
        //                    attendanceData = attendanceData.Where(a => a.Date >= startDate && a.Date <= endDate);
        //                }
        //                break;
        //        }
        //    }

        //    if (!string.IsNullOrEmpty(statusFilter))
        //    {
        //        // Convert both Status and statusFilter to lower case for case-insensitive comparison
        //        attendanceData = attendanceData.Where(a => a.Status.ToLower() == statusFilter.ToLower());
        //    }

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        attendanceData = attendanceData.Where(a => a.User.FirstName.Contains(searchTerm) || a.User.LastName.Contains(searchTerm));
        //    }

        //    // Sorting logic
        //    switch (sortOrder)
        //    {
        //        case "asc":
        //            attendanceData = attendanceData.OrderBy(a => a.Date);
        //            break;
        //        case "desc":
        //            attendanceData = attendanceData.OrderByDescending(a => a.Date);
        //            break;
        //        case "last7days":
        //            attendanceData = attendanceData.Where(a => a.Date >= DateTime.Today.AddDays(-7)).OrderByDescending(a => a.Date);
        //            break;
        //        case "lastmonth":
        //            DateTime today = DateTime.Today; // Declare today before using it
        //            DateTime firstDayOfLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        //            DateTime lastDayOfLastMonth = firstDayOfLastMonth.AddMonths(1).AddDays(-1);
        //            attendanceData = attendanceData
        //                .Where(a => a.Date >= firstDayOfLastMonth && a.Date <= lastDayOfLastMonth)
        //                .OrderByDescending(a => a.Date);
        //            break;

        //    }

        //    var result = attendanceData.Select(a => new
        //    {
        //        a.AttendanceId,
        //        a.UserId,
        //        UserName = a.User.FirstName + " " + a.User.LastName,
        //        a.User.ProfilePicture,
        //        a.Date,
        //        a.CheckIn,
        //        a.CheckOut,
        //        a.LunchIn,
        //        a.LunchOut,
        //        a.Status,
        //        a.WorkingHours,
        //        a.OvertimeHours,
        //        a.BreakHours,
        //        a.Late,
        //        a.ProductionHours
        //    }).ToList();

        //    return Json(result);
        //}
        public IActionResult GetFilteredReports(string dateFilter, string statusFilter, string sortOrder)
        {
            var attendanceQuery = db.Attendance
                .Include(a => a.User)
                .Select(a => new
                {
                    a.AttendanceId,
                    a.UserId,
                    UserName = a.User.FirstName + " " + a.User.LastName,
                    a.User.ProfilePicture,
                    a.Date,
                    a.CheckIn,
                    a.CheckOut,
                    a.LunchIn,
                    a.LunchOut,
                    a.Status,
                    a.WorkingHours,
                    a.OvertimeHours,
                    a.BreakHours,
                    a.Late,
                    a.ProductionHours
                });

            // **Apply Filters if Selected**
            if (!string.IsNullOrEmpty(dateFilter))
            {
                var dates = dateFilter.Split(" - ");
                if (dates.Length == 2 && DateTime.TryParse(dates[0], out DateTime startDate) && DateTime.TryParse(dates[1], out DateTime endDate))
                {
                    attendanceQuery = attendanceQuery.Where(a => a.Date >= startDate && a.Date <= endDate);
                }
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                attendanceQuery = attendanceQuery.Where(a => a.Status == statusFilter);
            }

            // **Sorting Logic**
            switch (sortOrder)
            {
                case "asc":
                    attendanceQuery = attendanceQuery.OrderBy(a => a.Date);
                    break;
                case "desc":
                    attendanceQuery = attendanceQuery.OrderByDescending(a => a.Date);
                    break;
                case "last7days":
                    attendanceQuery = attendanceQuery.Where(a => a.Date >= DateTime.Today.AddDays(-7)).OrderByDescending(a => a.Date);
                    break;
                case "lastmonth":
                    DateTime today = DateTime.Today; // Declare today before using it
                    DateTime firstDayOfLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    DateTime lastDayOfLastMonth = firstDayOfLastMonth.AddMonths(1).AddDays(-1);
                    attendanceQuery = attendanceQuery
                    .Where(a => a.Date >= firstDayOfLastMonth && a.Date <= lastDayOfLastMonth)
                    .OrderByDescending(a => a.Date);
                    break;

            }

            var attendanceList = attendanceQuery.ToList();

            return Json(attendanceList); // Return JSON response
        }



    }
}
