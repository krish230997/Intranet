using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.Text;


namespace Pulse360.Controllers
{
    public class AdminTimesheetController : Controller
    {
        private readonly ApplicationDbContext db;
        public AdminTimesheetController(ApplicationDbContext db)
        {
            this.db = db;
        }

        private IQueryable<Timesheet> GetRoleBasedTimesheets(User user)
        {
            var query = db.Timesheets
                .Include(t => t.User)
                .Include(t => t.Projects)
                .AsQueryable();

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                // Admins can access all records (no additional filter needed)
                return query;
            }
            else if (User.IsInRole("Manager"))
            {
                // Managers can access records of employees in their department
                return query.Where(t => t.User.DepartmentId == user.DepartmentId);
            }
            else
            {
                // Employees can only access their own records
                return query.Where(t => t.CreatedBy == user.Email);
            }
        }

        public IActionResult AdminTimesheet()
        {
            var username = User.Identity.Name;
            var user = db.User.Include(u => u.Department).FirstOrDefault(u => u.Email == username);

            if (user == null)
            {
                return RedirectToAction("Error", "Home", new { message = "User not found." });
            }

            //var timesheets = GetRoleBasedTimesheets(user)
            //    .Where(t => t.Status == "Pending Approval")
            //    .ToList();

            var timesheets = GetRoleBasedTimesheets(user).ToList();


            return View(timesheets);
        }

        // Bulk Approve Timesheets (via Form Data)
        [HttpPost]
        public IActionResult BulkApprove([FromForm] List<int> timesheetIds)
        {
            if (timesheetIds == null || !timesheetIds.Any())
            {
                TempData["error"] = "No timesheets selected for approval.";
                return RedirectToAction("AdminTimesheet");
            }

            // Get the logged-in user details
            var approverEmail = User.Identity.Name;
            var approver = db.User.FirstOrDefault(u => u.Email == approverEmail);

            if (approver == null)
            {
                TempData["error"] = "Unauthorized action.";
                return RedirectToAction("AdminTimesheet");
            }


            // Fetch role names dynamically from the database
            var adminRoleId = db.Role.FirstOrDefault(r => r.RoleName == "Admin")?.RoleId;
            var managerRoleId = db.Role.FirstOrDefault(r => r.RoleName == "Manager")?.RoleId;

            bool isAdmin = approver.RoleId == adminRoleId;
            bool isManager = approver.RoleId == managerRoleId;

            if (!isAdmin && !isManager)
            {
                TempData["error"] = "Only Admins and Managers can approve timesheets.";
                return RedirectToAction("AdminTimesheet");
            }

            foreach (var id in timesheetIds)
            {
                var timesheet = db.Timesheets.Include(t => t.User).FirstOrDefault(t => t.TimesheetId == id);
                if (timesheet != null)
                {
                    // If the approver is a manager, check department
                    if (isManager && timesheet.User.DepartmentId != approver.DepartmentId)
                    {
                        TempData["error"] = "Managers can only approve timesheets from their department.";
                        return RedirectToAction("AdminTimesheet");
                    }

                    timesheet.Status = "Approved";
                    timesheet.ApprovedBy = $"{approver.FirstName} {approver.LastName}".Trim(); // Set first name of the approver
                    timesheet.ApprovedAt = DateTime.Now;
                    db.Entry(timesheet).State = EntityState.Modified;
                }
            }

            db.SaveChanges();
            TempData["success"] = "Timesheets approved successfully.";
            return RedirectToAction("AdminTimesheet");
        }

        [HttpPost]
        public IActionResult BulkReject([FromForm] List<int> timesheetIds)
        {
            if (timesheetIds == null || !timesheetIds.Any())
            {
                TempData["error"] = "No timesheets selected for rejection.";
                return RedirectToAction("AdminTimesheet");
            }

            // Get the logged-in user details
            var approverEmail = User.Identity.Name;
            var approver = db.User.FirstOrDefault(u => u.Email == approverEmail);

            if (approver == null)
            {
                TempData["error"] = "Unauthorized action.";
                return RedirectToAction("AdminTimesheet");
            }

            // Fetch role names dynamically from the database
            var adminRoleId = db.Role.FirstOrDefault(r => r.RoleName == "Admin")?.RoleId;
            var managerRoleId = db.Role.FirstOrDefault(r => r.RoleName == "Manager")?.RoleId;

            bool isAdmin = approver.RoleId == adminRoleId;
            bool isManager = approver.RoleId == managerRoleId;

            if (!isAdmin && !isManager)
            {
                TempData["error"] = "Only Admins and Managers can reject timesheets.";
                return RedirectToAction("AdminTimesheet");
            }

            foreach (var id in timesheetIds)
            {
                var timesheet = db.Timesheets.Include(t => t.User).FirstOrDefault(t => t.TimesheetId == id);
                if (timesheet != null)
                {
                    // If the approver is a manager, check department
                    if (isManager && timesheet.User.DepartmentId != approver.DepartmentId)
                    {
                        TempData["error"] = "Managers can only reject timesheets from their department.";
                        return RedirectToAction("AdminTimesheet");
                    }

                    timesheet.Status = "Rejected";
                    timesheet.ApprovedBy = $"{approver.FirstName} {approver.LastName}".Trim(); // Set first name of the approver
                    timesheet.ApprovedAt = DateTime.Now;
                    db.Entry(timesheet).State = EntityState.Modified;
                }
            }

            db.SaveChanges();
            TempData["success"] = "Timesheets rejected successfully.";
            return RedirectToAction("AdminTimesheet");
        }

        public IActionResult ExportTimesheets(string status, string search, string project)
        {
            var username = User.Identity.Name;
            var user = db.User.Include(u => u.Department).FirstOrDefault(u => u.Email == username);

            if (user == null)
            {
                return RedirectToAction("Error", "Home", new { message = "User not found." });
            }

            var query = GetRoleBasedTimesheets(user);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.User.Email.Contains(search) || t.Projects.ProjectName.Contains(search));

            if (!string.IsNullOrEmpty(project))
                query = query.Where(t => t.Projects.ProjectName.Contains(project));

            var timesheets = query.ToList();

            var csvData = new StringBuilder();
            csvData.AppendLine("FirstName,LastName,Email,CreatedAt,Project,WorkHours,Status,ApprovedBy,ApprovedAt");

            foreach (var t in timesheets)
            {
                csvData.AppendLine($"{t.User?.FirstName ?? "N/A"},{t.User?.LastName ?? "N/A"},{t.User?.Email ?? "N/A"},{t.Date.ToShortDateString()},{t.Projects?.ProjectName ?? "N/A"},{t.WorkHours},{t.Status},{t.ApprovedBy ?? "N/A"},{(t.ApprovedAt.HasValue ? t.ApprovedAt.Value.ToShortDateString() : "N/A")}");
            }

            var fileBytes = Encoding.UTF8.GetBytes(csvData.ToString());
            return File(fileBytes, "text/csv", "Timesheets.csv");
        }

        public IActionResult ExportTimesheetsPDF(string status, string search, string project)
        {
            var username = User.Identity.Name;
            var user = db.User.Include(u => u.Department).FirstOrDefault(u => u.Email == username);

            if (user == null)
            {
                return RedirectToAction("Error", "Home", new { message = "User not found." });
            }

            var query = GetRoleBasedTimesheets(user);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.User.Email.Contains(search) || t.Projects.ProjectName.Contains(search));

            if (!string.IsNullOrEmpty(project))
                query = query.Where(t => t.Projects.ProjectName.Contains(project));

            var timesheets = query.ToList();

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 10f, 10f, 20f, 20f);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add Title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Paragraph title = new Paragraph("Timesheets Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(title);

                // Create Table
                PdfPTable table = new PdfPTable(8)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 1.5f, 1.5f, 2.5f, 2.5f, 2.5f, 1.5f, 1.5f, 2.5f });

                // Add Table Headers
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                string[] headers = { "First Name", "Last Name", "Email", "Date", "Project", "Hours", "Status", "Approved By" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Add Table Data
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                foreach (var t in timesheets)
                {
                    table.AddCell(new PdfPCell(new Phrase(t.User?.FirstName ?? "N/A", dataFont)));
                    table.AddCell(new PdfPCell(new Phrase(t.User?.LastName ?? "N/A", dataFont)));
                    table.AddCell(new PdfPCell(new Phrase(t.User?.Email ?? "N/A", dataFont)));
                    table.AddCell(new PdfPCell(new Phrase(t.Date.ToShortDateString(), dataFont)));
                    table.AddCell(new PdfPCell(new Phrase(t.Projects?.ProjectName ?? "N/A", dataFont)));
                    table.AddCell(new PdfPCell(new Phrase(t.WorkHours.ToString(), dataFont)));
                    table.AddCell(new PdfPCell(new Phrase(t.Status, dataFont)));
                    table.AddCell(new PdfPCell(new Phrase(t.ApprovedBy ?? "N/A", dataFont)));
                }

                document.Add(table);
                document.Close();

                byte[] bytes = stream.ToArray();
                return File(bytes, "application/pdf", "Timesheets.pdf");
            }
        }

        public IActionResult FilteredTimesheets(string status, string search, string project)
        {
            var username = User.Identity.Name;
            var user = db.User.Include(u => u.Department).FirstOrDefault(u => u.Email == username);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var query = GetRoleBasedTimesheets(user);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            else
            {
                query = query.Where(t => t.Status != "Pending");
            }
            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.User.Email.Contains(search) || t.Projects.ProjectName.Contains(search));

            if (!string.IsNullOrEmpty(project))
                query = query.Where(t => t.Projects.ProjectName.Contains(project));

            var result = query.Select(t => new
            {
                t.TimesheetId,
                UserFirstName = t.User.FirstName,
                UserLastName = t.User.LastName,
                UserProfilePicture = t.User.ProfilePicture,
                t.Date,
                ProjectName = t.Projects.ProjectName ?? "N/A",
                t.WorkHours,
                t.Status
            }).ToList();

            return Json(result);
        }
    }
}
