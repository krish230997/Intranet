using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class ActivityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Activity Index
        [HttpGet]
        public IActionResult Index(string statusFilter, string sortByDate)
        {
            var activities = _context.Activiti.AsQueryable();

            // Filter by Status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                activities = activities.Where(a => a.Status == statusFilter);
            }

            // Sort by Date (Ascending or Descending)
            if (sortByDate == "asc")
            {
                activities = activities.OrderBy(a => a.DueDate);
            }
            else if (sortByDate == "desc")
            {
                activities = activities.OrderByDescending(a => a.DueDate);
            }

            return View(activities.ToList());
        }


        // POST: Create Activity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Activitys activity)
        {
            if (ModelState.IsValid)
            {
                activity.CreatedDate = DateTime.Now;
                _context.Activiti.Add(activity);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Activitys activity)
        {
            if (ModelState.IsValid)
            {
                _context.Activiti.Update(activity);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int ActivityId)
        {
            var activity = _context.Activiti    .Find(ActivityId);
            if (activity != null)
            {
                _context.Activiti.Remove(activity);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // Export to Excel
        public IActionResult ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context

            var activities = _context.Activiti.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Activities");
                worksheet.Cells[1, 1].Value = "Title";
                worksheet.Cells[1, 2].Value = "Type";
                worksheet.Cells[1, 3].Value = "Due Date";
                worksheet.Cells[1, 4].Value = "Owner";
                worksheet.Cells[1, 5].Value = "Status";
                worksheet.Cells[1, 6].Value = "Created Date";

                int row = 2;
                foreach (var activity in activities)
                {
                    worksheet.Cells[row, 1].Value = activity.Title;
                    worksheet.Cells[row, 2].Value = activity.ActivityType;
                    worksheet.Cells[row, 3].Value = activity.DueDate.ToShortDateString();
                    worksheet.Cells[row, 4].Value = activity.Owner;
                    worksheet.Cells[row, 5].Value = activity.Status;
                    worksheet.Cells[row, 6].Value = activity.CreatedDate.ToShortDateString();
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Activities-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // Export to PDF
        public IActionResult ExportToPdf()
        {
            var activities = _context.Activiti.ToList();
            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter.GetInstance(document, stream);
                document.Open();
                PdfPTable table = new PdfPTable(6);

                table.AddCell("Title");
                table.AddCell("Type");
                table.AddCell("Due Date");
                table.AddCell("Owner");
                table.AddCell("Status");
                table.AddCell("Created Date");

                foreach (var activity in activities)
                {
                    table.AddCell(activity.Title);
                    table.AddCell(activity.ActivityType);
                    table.AddCell(activity.DueDate.ToShortDateString());
                    table.AddCell(activity.Owner);
                    table.AddCell(activity.Status);
                    table.AddCell(activity.CreatedDate.ToShortDateString());
                }

                document.Add(table);
                document.Close();

                byte[] pdfBytes = stream.ToArray();
                string pdfName = $"Activities-{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(pdfBytes, "application/pdf", pdfName);
            }
        }
    }
}
