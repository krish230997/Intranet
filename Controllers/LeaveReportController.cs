using ClosedXML.Excel;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse360.Data;

namespace Pulse360.Controllers
{
    public class LeaveReportController : Controller
    {
        private readonly ApplicationDbContext db;
        public LeaveReportController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult LeaveReport()
        {
            var totalLeave = db.MasterLeaveTypes.Count();

            // Approved Leave count
            var approvedLeave = db.LeaveRequests.Count(lr => lr.Status == "Approved");

            // Pending Leave count
            var pendingLeave = db.LeaveRequests.Count(lr => lr.Status == "Pending");

            // Rejected Leave count
            var rejectedLeave = db.LeaveRequests.Count(lr => lr.Status == "Rejected");

            // Passing data to the ViewBag to use directly in the view
            ViewBag.TotalLeave = totalLeave;
            ViewBag.ApprovedLeave = approvedLeave;
            ViewBag.PendingLeave = pendingLeave;
            ViewBag.RejectedLeave = rejectedLeave;


            var leaveRequests = db.LeaveRequests
              .Include(l => l.User) // Include User data
              .Include(l => l.MasterLeaveType)
              .Select(l => new
              {
                  l.LeaveRequestId,
                  l.User.ProfilePicture,
                  UserName = l.User.FirstName + " " + l.User.LastName,
                  LeaveType = l.MasterLeaveType != null ? l.MasterLeaveType.LeaveType : "N/A",
                  l.StartDate,
                  l.EndDate,
                  l.Status,
                  l.NumberOfDays,
                  l.Reason,
                  l.ApprovedBy,
                  l.StatusHistory

              })
              .ToList(); // Using synchronous ToList()

            var leaveData = db.LeaveRequests
               .Include(l => l.MasterLeaveType) // Assuming a navigation property exists
               .GroupBy(l => new { Year = l.StartDate.Year, l.MasterLeaveType.LeaveType })
               .Select(g => new
               {
                   Year = g.Key.Year,
                   LeaveType = g.Key.LeaveType,
                   LeaveCount = g.Sum(l => l.NumberOfDays)
               })
               .ToList();
            var jsondata = JsonConvert.SerializeObject(leaveData);
            ViewBag.JsonData = jsondata;

            return View(leaveRequests);
        }
        public IActionResult ExportToPdf()
        {
            // Fetch attendance data
            var leaveRequests = db.LeaveRequests
             .Include(l => l.User) // Include User data
             .Include(l => l.MasterLeaveType)
             .Select(l => new
             {
                 l.LeaveRequestId,
                 l.User.ProfilePicture,
                 UserName = l.User.FirstName + " " + l.User.LastName,
                 LeaveType = l.MasterLeaveType != null ? l.MasterLeaveType.LeaveType : "N/A",
                 l.StartDate,
                 l.EndDate,
                 l.Status,
                 l.NumberOfDays,
                 l.Reason,
                 ApprovedBy = l.User.FirstName + " " + l.User.LastName,
                 l.StatusHistory
                 //UserName = l.User.FirstName + " " + l.User.LastName

             })
             .ToList(); // Using synchronous ToList()


            // Create a new memory stream to hold the PDF
            using (var stream = new MemoryStream())
            {
                // Create a PdfWriter instance to write the document to the memory stream
                using (var writer = new PdfWriter(stream))
                {
                    // Create the PdfDocument instance
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());
                    document.SetMargins(10, 10, 10, 10);

                    // Add title to the document with a default font
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    document.Add(new Paragraph("Leave Report")
                        .SetFont(font)
                        .SetFontSize(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginBottom(20));



                    // Create a table to hold the data
                    var table = new Table(UnitValue.CreatePercentArray(10)); // 13 columns
                    table.UseAllAvailableWidth();
                    // Add header row
                    table.AddCell("LeaveRequestId");
                    table.AddCell("UserName");
                    table.AddCell("LeaveType");
                    table.AddCell("StartDate");
                    table.AddCell("EndDate");
                    table.AddCell("Status");
                    table.AddCell("NumberOfDays");
                    table.AddCell("Reason");
                    table.AddCell("ApprovedBy");
                    table.AddCell("StatusHistory");

                    // Add data rows
                    foreach (var item in leaveRequests)
                    {
                        table.AddCell(item.LeaveRequestId.ToString());
                        table.AddCell(item.UserName);
                        table.AddCell(item.LeaveType);
                        table.AddCell(item.StartDate.ToString("yyyy-MM-dd"));
                        table.AddCell(item.EndDate.ToString("yyyy-MM-dd"));
                        table.AddCell(item.Status);
                        table.AddCell(item.NumberOfDays.ToString());
                        table.AddCell(item.Reason);
                        table.AddCell(item.ApprovedBy);
                        table.AddCell(item.StatusHistory);

                    }

                    // Add table to the document
                    document.Add(table);

                    // Close the document
                    document.Close();
                }

                // Return the PDF as a downloadable file
                return File(stream.ToArray(), "application/pdf", "LeaveReport.pdf");
            }
        }
        public IActionResult ExportToExcel()
        {
            // Fetch attendance data
            var leaveRequests = db.LeaveRequests
             .Include(l => l.User) // Include User data
             .Include(l => l.MasterLeaveType)
             .Select(l => new
             {
                 l.LeaveRequestId,
                 LeaveType = l.MasterLeaveType != null ? l.MasterLeaveType.LeaveType : "N/A",
                 l.StartDate,
                 l.EndDate,
                 l.Status,
                 l.NumberOfDays,
                 l.Reason,
                 l.ApprovedBy,
                 l.StatusHistory,
                 UserName = l.User.FirstName + " " + l.User.LastName
                 //UserName = l.User.Username
             })
             .ToList(); // Using synchronous ToList()

            // Create a new Excel workbook
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet("Leave Report");

                // Set headers
                worksheet.Cell(1, 1).Value = "LeaveRequestId";
                worksheet.Cell(1, 2).Value = "LeaveType";
                worksheet.Cell(1, 3).Value = "StartDate";
                worksheet.Cell(1, 4).Value = "EndDate";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "NumberOfDays";
                worksheet.Cell(1, 7).Value = "Reason";
                worksheet.Cell(1, 8).Value = "ApprovedBy";
                worksheet.Cell(1, 9).Value = "StatusHistory";
                worksheet.Cell(1, 10).Value = "ProfilePhotoWithFullName";


                // Add data rows
                int row = 2;
                foreach (var item in leaveRequests)
                {
                    worksheet.Cell(row, 1).Value = item.LeaveRequestId;
                    worksheet.Cell(row, 2).Value = item.LeaveType;
                    worksheet.Cell(row, 3).Value = item.StartDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = item.EndDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 5).Value = item.Status;
                    worksheet.Cell(row, 6).Value = item.NumberOfDays;
                    worksheet.Cell(row, 7).Value = item.Reason;
                    worksheet.Cell(row, 8).Value = item.ApprovedBy;
                    worksheet.Cell(row, 9).Value = item.StatusHistory;
                    worksheet.Cell(row, 10).Value = item.UserName;

                    row++;
                }

                // Save the Excel file to a memory stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    // Return the Excel file as a downloadable file
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LeaveReport.xlsx");
                }
            }

        }
        [HttpGet]

        public IActionResult GetFilteredReports(string dateFilter, string statusFilter, string sortOrder, DateTime? startDate, DateTime? endDate, string searchTerm)
        {
            var leaveData = db.LeaveRequests.Include(a => a.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(dateFilter))
            {
                DateTime today = DateTime.Today;
                switch (dateFilter.ToLower())
                {
                    case "yesterday":
                        leaveData = leaveData.Where(a => a.StartDate == today.AddDays(-1));
                        break;
                    case "last7days":
                        leaveData = leaveData.Where(a => a.StartDate >= today.AddDays(-7));
                        break;
                    case "last30days":
                        leaveData = leaveData.Where(a => a.StartDate >= today.AddDays(-30));
                        break;
                    case "lastyear":
                        leaveData = leaveData.Where(a => a.StartDate.Year == today.Year - 1);
                        break;
                    case "thisyear":
                        leaveData = leaveData.Where(a => a.StartDate.Year == today.Year);
                        break;
                    case "customrange":
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            leaveData = leaveData.Where(a => a.StartDate >= startDate && a.StartDate <= endDate);
                        }
                        break;
                }
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                // Convert both Status and statusFilter to lower case for case-insensitive comparison
                leaveData = leaveData.Where(a => a.Status.ToLower() == statusFilter.ToLower());
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                leaveData = leaveData.Where(a => a.User.FirstName.Contains(searchTerm) || a.User.LastName.Contains(searchTerm));
                //leaveData = leaveData.Where(a => a.User.FirstName.Contains(searchTerm));
            }

            // Sorting logic
            switch (sortOrder)
            {
                case "asc":
                    leaveData = leaveData.OrderBy(a => a.StartDate);
                    break;
                case "desc":
                    leaveData = leaveData.OrderByDescending(a => a.StartDate);
                    break;
                case "last7days":
                    leaveData = leaveData.Where(a => a.StartDate >= DateTime.Today.AddDays(-7)).OrderByDescending(a => a.StartDate);
                    break;
                case "lastmonth":
                    DateTime today = DateTime.Today; // Declare today before using it
                    DateTime firstDayOfLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    DateTime lastDayOfLastMonth = firstDayOfLastMonth.AddMonths(1).AddDays(-1);
                    leaveData = leaveData
                        .Where(a => a.StartDate >= firstDayOfLastMonth && a.StartDate <= lastDayOfLastMonth)
                        .OrderByDescending(a => a.StartDate);
                    break;

            }

            var result = leaveData.Select(a => new
            {
                a.LeaveRequestId,
                a.User.ProfilePicture,
                UserName = a.User.FirstName + " " + a.User.LastName,
                LeaveType = a.MasterLeaveType != null ? a.MasterLeaveType.LeaveType : "N/A",
                a.StartDate,
                a.EndDate,
                a.NumberOfDays,
                a.Reason,
                ApprovedBy = a.User.FirstName,
                a.Status,
                a.StatusHistory
                //UserName = l.User.FirstName + " " + l.User.LastName

            }).ToList();

            return Json(result);
        }


    }
}
