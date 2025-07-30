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

namespace PayslipReportcore.Controllers
{
    public class PaySlipReportController : Controller
    {
        private readonly ApplicationDbContext db;
        public PaySlipReportController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index(string monthFilter, string sortOrder)
        {
            var totalSalary = db.EmployeeSalaries.Sum(e => e.TotalSalary);
            var totalDeductions = db.EmployeeDeductions.Sum(d => d.DeductionAmount);
            var netSalary = totalSalary - totalDeductions;
            var earnings = db.EmployeeEarnings.Sum(e => e.EarningAmount);

            ViewBag.TotalSalary = totalSalary;
            ViewBag.TotalDeductions = totalDeductions;
            ViewBag.NetSalary = netSalary;
            ViewBag.Earnings = earnings;

            var payslipQuery = db.Payslips
                .Join(db.EmployeeSalaries,
                    payslip => payslip.UserId,
                    earning => earning.UserId,
                    (payslip, earning) => new
                    {
                        PayslipId = payslip.PayslipId,
                        UserName = payslip.User.FirstName + " " + payslip.User.LastName,
                        Designation = payslip.User.Designation,
                        ProfilePicture = payslip.User.ProfilePicture,
                        PaidAmount = earning.TotalSalary,
                        PaidMonth = payslip.Month,
                        PaidYear = payslip.Year
                    });

            // Fetch all data from the database first
            var payslips = payslipQuery.ToList();

            var monthMapping = new Dictionary<string, int>
    {
        { "January", 1 },
        { "February", 2 },
        { "March", 3 },
        { "April", 4 },
        { "May", 5 },
        { "June", 6 },
        { "July", 7 },
        { "August", 8 },
        { "September", 9 },
        { "October", 10 },
        { "November", 11 },
        { "December", 12 }
    };

            // Filter based on selected month range
            if (!string.IsNullOrEmpty(monthFilter))
            {
                int monthStart = 0;
                int monthEnd = 0;

                switch (monthFilter)
                {
                    case "Jan-March":
                        monthStart = 1;
                        monthEnd = 3;
                        break;
                    case "April-Jun":
                        monthStart = 4;
                        monthEnd = 6;
                        break;
                    case "July-Sept":
                        monthStart = 7;
                        monthEnd = 9;
                        break;
                    case "Oct-Dec":
                        monthStart = 10;
                        monthEnd = 12;
                        break;
                }

                // Apply the filter locally, using the monthMapping
                payslips = payslips.Where(p =>
                    monthMapping.ContainsKey(p.PaidMonth) &&
                    monthMapping[p.PaidMonth] >= monthStart &&
                    monthMapping[p.PaidMonth] <= monthEnd)
                    .ToList();
            }

            // Sorting logic
            if (!string.IsNullOrEmpty(sortOrder))
            {
                switch (sortOrder)
                {
                    case "Ascending":
                        payslips = payslips.OrderBy(p => p.PaidYear).ToList();
                        break;
                    case "Descending":
                        payslips = payslips.OrderByDescending(p => p.PaidYear).ToList();
                        break;
                }
            }

            // Prepare the report to pass to the view
            var payslipReport = payslips
                .Select(p => new
                {
                    PayslipId = p.PayslipId,
                    UserName = p.UserName,
                    Designation = p.Designation,
                    ProfilePicture = p.ProfilePicture,
                    PaidAmount = p.PaidAmount,
                    PaidMonth = p.PaidMonth,
                    PaidYear = p.PaidYear
                })
                .ToList();

            return View(payslipReport);
        }

        public IActionResult ExportToPdf()
        {
            // Fetch attendance data
            var payslipReport = db.Payslips
      .Join(db.EmployeeSalaries,
          payslip => payslip.UserId,
          earning => earning.UserId,
          (payslip, earning) => new
          {
              PayslipId = payslip.PayslipId,
              UserName = payslip.User.FirstName + " " + payslip.User.LastName, // Combine First and Last name
              Designation = payslip.User.Designation,
              ProfilePicture = payslip.User.ProfilePicture,
              PaidAmount = earning.TotalSalary,
              PaidMonth = payslip.Month,
              PaidYear = payslip.Year
          })
      .ToList()
      .Select(p => new
      {
          PayslipId = p.PayslipId,
          UserName = p.UserName,
          Designation = p.Designation,
          ProfilePicture = p.ProfilePicture,
          PaidAmount = p.PaidAmount,
          PaidMonth = p.PaidMonth,
          PaidYear = p.PaidYear
      })
      .ToList();

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
                    var table = new Table(UnitValue.CreatePercentArray(6)); // 13 columns
                    table.UseAllAvailableWidth();
                    // Add header row
                    table.AddCell("PayslipId");
                    table.AddCell("UserName");
                    table.AddCell("ProfilePicture");
                    table.AddCell("PaidAmount");
                    table.AddCell("PaidMonth");
                    table.AddCell("PaidYear");


                    // Add data rows
                    foreach (var item in payslipReport)
                    {
                        table.AddCell(item.PayslipId.ToString());
                        table.AddCell(item.UserName);
                        table.AddCell(item.ProfilePicture);
                        table.AddCell(item.PaidAmount.ToString());
                        table.AddCell(item.PaidMonth);
                        table.AddCell(item.PaidYear.ToString());

                    }

                    // Add table to the document
                    document.Add(table);

                    // Close the document
                    document.Close();
                }

                // Return the PDF as a downloadable file
                return File(stream.ToArray(), "application/pdf", "PaySlipReport.pdf");
            }
        }
        public IActionResult ExportToExcel()
        {
            var payslipReport = db.Payslips
  .Join(db.EmployeeSalaries,
      payslip => payslip.UserId,
      earning => earning.UserId,
      (payslip, earning) => new
      {
          PayslipId = payslip.PayslipId,
          UserName = payslip.User.FirstName + " " + payslip.User.LastName, // Combine First and Last name
          ProfilePicture = payslip.User.ProfilePicture,
          PaidAmount = earning.TotalSalary,
          PaidMonth = payslip.Month,
          PaidYear = payslip.Year
      })
  .ToList()
  .Select(p => new
  {
      PayslipId = p.PayslipId,
      UserName = p.UserName,
      ProfilePicture = p.ProfilePicture,
      PaidAmount = p.PaidAmount,
      PaidMonth = p.PaidMonth,
      PaidYear = p.PaidYear
  })
  .ToList();


            // Create a new Excel workbook
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.AddWorksheet("Leave Report");

                // Set headers
                worksheet.Cell(1, 1).Value = "PayslipId";
                worksheet.Cell(1, 2).Value = "UserName";
                worksheet.Cell(1, 3).Value = "ProfilePicture";
                worksheet.Cell(1, 4).Value = "PaidAmount";
                worksheet.Cell(1, 5).Value = "PaidMonth";
                worksheet.Cell(1, 6).Value = "PaidYear";
                // Add data rows
                int row = 2;
                foreach (var item in payslipReport)
                {
                    worksheet.Cell(row, 1).Value = item.PayslipId;
                    worksheet.Cell(row, 2).Value = item.UserName;
                    worksheet.Cell(row, 3).Value = item.ProfilePicture;
                    worksheet.Cell(row, 4).Value = item.PaidAmount.ToString();
                    worksheet.Cell(row, 5).Value = item.PaidMonth;
                    worksheet.Cell(row, 6).Value = item.PaidYear.ToString();

                    row++;
                }

                // Save the Excel file to a memory stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    // Return the Excel file as a downloadable file
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PaySlipReport.xlsx");
                }
            }
        }
        public JsonResult GetSalaryGraphData()
        {
            // Query to get the total salary per year
            var salaryData = db.Payslips
                .Join(db.EmployeeSalaries,
                      payslip => payslip.UserId,
                      salary => salary.UserId,
                      (payslip, salary) => new { payslip.Year, salary.TotalSalary })
                .GroupBy(x => x.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalSalary = g.Sum(x => x.TotalSalary)
                })
                .OrderBy(x => x.Year)
                .ToList();

            // Return the data as JSON
            return Json(salaryData);
        }

    }
}
