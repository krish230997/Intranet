using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using Pulse360.Data;
using Microsoft.EntityFrameworkCore;

namespace Pulse360.Controllers
{
    public class ProjectTaskReportController : Controller
    {
        private readonly ApplicationDbContext db;

        public ProjectTaskReportController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult ProjectReport()
        //{
        //    var data = db.AllProjects
        //        .Include(p => p.Users)
        //        .Select(p => new
        //        {
        //            p.ProjectId,
        //            p.ProjectName,
        //            p.ManagerName,
        //            Members = p.Users.Select(u => u.ProfilePicture ?? "default.png" ).ToList(),
        //            p.EndDate,
        //            p.Priority,
        //            p.Status    
        //        }).ToList();

        //    var totalProjects = db.AllProjects.Count();
        //    var completedProjects = db.AllProjects.Count(p => p.Status == "Active");
        //    var overdueProjects = db.AllProjects.Count(p => p.Status == "Inactive");
        //    var onHoldProjects = db.Task.Count(p => p.Status == "Completed");
        //    var activeProjects = db.Task.Count(p => p.Status == "Inprogress");
        //    // Avoid division by zero when calculating percentages
        //    double completedPercentage = totalProjects > 0 ? (completedProjects * 100.0) / totalProjects : 0;
        //    double overduePercentage = totalProjects > 0 ? (overdueProjects * 100.0) / totalProjects : 0;
        //    double onHoldPercentage = totalProjects > 0 ? (onHoldProjects * 100.0) / totalProjects : 0;
        //    double activePercentage = totalProjects > 0 ? (activeProjects * 100.0) / totalProjects : 0;

        //    var taskData = db.Task
        //        .GroupBy(t => t.Status)
        //        .Select(g => new
        //        {
        //            Status = g.Key,
        //            Count = g.Count()
        //        })
        //        .ToList();

        //    // Convert the task data to JSON format
        //    var jsonData = JsonConvert.SerializeObject(taskData);

        //    // Pass JSON data to the view
        //    ViewBag.JsonData = jsonData;

        //    ViewBag.TotalProjects = totalProjects;
        //    ViewBag.CompletedProjects = completedProjects;
        //    ViewBag.OverdueProjects = overdueProjects;
        //    ViewBag.OnHoldProjects = onHoldProjects;
        //    ViewBag.ActiveProjects = activeProjects;
        //    ViewBag.CompletedPercentage = completedPercentage;
        //    ViewBag.OverduePercentage = overduePercentage;
        //    ViewBag.OnHoldPercentage = onHoldPercentage;
        //    ViewBag.ActivePercentage = activePercentage;

        //    return View(data);
        //}
        public IActionResult ProjectReport(string priority = null, string status = null, string sort = null)
        {
            Console.WriteLine($"Priority: {priority}, Status: {status}, Sort: {sort}"); // Debugging Output

            var query = db.AllProjects.Include(p => p.Users).AsQueryable();

            if (!string.IsNullOrEmpty(priority))
            {
                query = query.Where(p => p.Priority == priority);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var data = query.Select(p => new
            {
                p.ProjectId,
                p.ProjectName,
                p.ManagerName,
                Members = p.Users.Select(u => u.ProfilePicture ?? "default.png").ToList(),
                p.EndDate,
                p.Priority,
                p.Status
            }).ToList();

            if (sort == "Ascending")
            {
                data = data.OrderBy(p => p.ProjectName).ToList();
            }
            else if (sort == "Descending")
            {
                data = data.OrderByDescending(p => p.ProjectName).ToList();
            }

            var totalProjects = db.AllProjects.Count();
            var completedProjects = db.AllProjects.Count(p => p.Status == "Active");
            var overdueProjects = db.AllProjects.Count(p => p.Status == "Inactive");
            var onHoldProjects = db.Task.Count(p => p.Status == "Completed");
            var activeProjects = db.Task.Count(p => p.Status == "Inprogress");

            double completedPercentage = totalProjects > 0 ? (completedProjects * 100.0) / totalProjects : 0;
            double overduePercentage = totalProjects > 0 ? (overdueProjects * 100.0) / totalProjects : 0;
            double onHoldPercentage = totalProjects > 0 ? (onHoldProjects * 100.0) / totalProjects : 0;
            double activePercentage = totalProjects > 0 ? (activeProjects * 100.0) / totalProjects : 0;

            var taskData = db.Task
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var jsonData = JsonConvert.SerializeObject(taskData);
            ViewBag.JsonData = jsonData;
            ViewBag.TotalProjects = totalProjects;
            ViewBag.CompletedProjects = completedProjects;
            ViewBag.OverdueProjects = overdueProjects;
            ViewBag.OnHoldProjects = onHoldProjects;
            ViewBag.ActiveProjects = activeProjects;
            ViewBag.CompletedPercentage = completedPercentage;
            ViewBag.OverduePercentage = overduePercentage;
            ViewBag.OnHoldPercentage = onHoldPercentage;
            ViewBag.ActivePercentage = activePercentage;

            return View(data);
        }
        public IActionResult ExportToPDF()
        {
            var projects = db.AllProjects
                .Include(p => p.Users)
                .ToList();

            using (var ms = new MemoryStream())
            {
                // Create a Document object
                var document = new Document(PageSize.A4);

                // Create a PdfWriter instance that writes to the memory stream
                var writer = PdfWriter.GetInstance(document, ms);

                // Open the document to start adding content
                document.Open();

                // Add a title to the document
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new iTextSharp.text.Paragraph("Project Report", titleFont) { Alignment = Element.ALIGN_CENTER };
                document.Add(title);
                document.Add(new iTextSharp.text.Paragraph(" ")); // Blank line

                // Create a table with 7 columns
                var table = new PdfPTable(7)
                {
                    WidthPercentage = 100
                };

                // Add table headers
                table.AddCell("Project ID");
                table.AddCell("Project Name");
                table.AddCell("Leader");
                table.AddCell("Members");
                table.AddCell("Deadline");
                table.AddCell("Priority");
                table.AddCell("Status");

                // Add data rows from the projects
                foreach (var p in projects)
                {
                    table.AddCell(p.ProjectId.ToString());
                    table.AddCell(p.ProjectName);
                    table.AddCell(p.ManagerName);
                    var members = string.Join(", ", p.Users.Select(u => u.FirstName + " " + u.LastName));
                    table.AddCell(members);
                    table.AddCell(p.EndDate.ToString("dd/MM/yyyy"));
                    table.AddCell(p.Priority);
                    table.AddCell(p.Status);
                }

                // Add the table to the document
                document.Add(table);

                // Close the document
                document.Close();

                // Return the file as a PDF download
                return File(ms.ToArray(), "application/pdf", "Project Report.pdf");
            }
        }

        // Export to Excel (CSV)
        public IActionResult ExportToExcel()
        {
            var projects = db.AllProjects
                .Include(p => p.Users)
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Projects");

                // Add header
                worksheet.Cells[1, 1].Value = "Project ID";
                worksheet.Cells[1, 2].Value = "Project Name";
                worksheet.Cells[1, 3].Value = "Leader";
                worksheet.Cells[1, 4].Value = "Members";
                worksheet.Cells[1, 5].Value = "Deadline";
                worksheet.Cells[1, 6].Value = "Priority";
                worksheet.Cells[1, 7].Value = "Status";

                // Add rows
                int row = 2;
                foreach (var p in projects)
                {
                    worksheet.Cells[row, 1].Value = p.ProjectId;
                    worksheet.Cells[row, 2].Value = p.ProjectName;
                    worksheet.Cells[row, 3].Value = p.ManagerName;
                    // Get the members' full names (FirstName + LastName)
                    var members = string.Join(", ", p.Users.Select(u => u.FirstName + " " + u.LastName));
                    worksheet.Cells[row, 4].Value = members;
                    worksheet.Cells[row, 5].Value = p.EndDate.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 6].Value = p.Priority;
                    worksheet.Cells[row, 7].Value = p.Status;
                    row++;
                }

                // Save the file to a MemoryStream
                var fileBytes = package.GetAsByteArray();

                // Return the file as an Excel download
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Project Report.xlsx");
            }

        }

        public IActionResult TaskReport(string priority = null, string status = null, string sort = null)
        {
            var tasksQuery = db.Task
                .Include(t => t.Project)
                .Include(t => t.Taskmember)
                    .ThenInclude(tm => tm.User)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrEmpty(priority))
            {
                tasksQuery = tasksQuery.Where(t => t.Priority == priority);
            }

            if (!string.IsNullOrEmpty(status))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == status);
            }

            // Apply Sorting
            switch (sort)
            {
                case "Recently Added":
                    tasksQuery = tasksQuery.Where(t => t.Deadline != DateTime.MinValue)
                             .OrderByDescending(t => t.Deadline);
                    break;
                case "Ascending":
                    tasksQuery = tasksQuery.OrderBy(t => t.Title);
                    break;
                case "Descending":
                    tasksQuery = tasksQuery.OrderByDescending(t => t.Title);
                    break;
                case "Last Month":
                    tasksQuery = tasksQuery.Where(t => t.Deadline != DateTime.MinValue &&
                                                       t.Deadline >= DateTime.Now.AddMonths(-1));
                    break;
                case "Last 7 Days":
                    tasksQuery = tasksQuery.Where(t => t.Deadline != DateTime.MinValue &&
                                                       t.Deadline >= DateTime.Now.AddDays(-7));
                    break;
            }

            var tasks = tasksQuery.ToList();

            // Task Grouping for JSON Data
            var taskData = tasks
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.JsonData = JsonConvert.SerializeObject(taskData);

            // Compute statistics
            int totalTasks = tasks.Count();
            int completedTasks = tasks.Count(t => t.Status == "Completed");
            int overdueTasks = tasks.Count(t => t.Status == "Pending");
            int inProgressTasks = tasks.Count(t => t.Status == "Inprogress");
            int onHoldTasks = tasks.Count(t => t.Status == "Onhold");

            // Store statistics in ViewBag
            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedPercentage = totalTasks > 0 ? (completedTasks * 100 / totalTasks) : 0;
            ViewBag.OverduePercentage = totalTasks > 0 ? (overdueTasks * 100 / totalTasks) : 0;
            ViewBag.InProgressPercentage = totalTasks > 0 ? (inProgressTasks * 100 / totalTasks) : 0;
            ViewBag.OnHoldPercentage = totalTasks > 0 ? (onHoldTasks * 100 / totalTasks) : 0;

            ViewBag.Completed = completedTasks;
            ViewBag.Overdue = overdueTasks;
            ViewBag.InProgress = inProgressTasks;
            ViewBag.OnHold = onHoldTasks;

            return View(tasks);
        }

        public IActionResult TaskExportToPDF()
        {
            var tasks = db.Task.Include(t => t.Project).ToList();

            using (var ms = new MemoryStream())
            {
                // Create a Document object
                var document = new Document(PageSize.A4);

                // Create a PdfWriter instance that writes to the memory stream
                var writer = PdfWriter.GetInstance(document, ms);

                // Open the document to start adding content
                document.Open();

                // Add a title to the document
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new iTextSharp.text.Paragraph("Task Report", titleFont) { Alignment = Element.ALIGN_CENTER };
                document.Add(title);
                document.Add(new iTextSharp.text.Paragraph(" ")); // Blank line

                // Create a table with 6 columns
                var table = new PdfPTable(6)
                {
                    WidthPercentage = 100
                };

                // Add table headers
                table.AddCell("Task ID");
                table.AddCell("Task Name");
                table.AddCell("Project Name");
                table.AddCell("Due Date");
                table.AddCell("Priority");
                table.AddCell("Status");

                // Add data rows from the projects
                foreach (var p in tasks)
                {
                    table.AddCell(p.TaskId.ToString());
                    table.AddCell(p.Title);
                    table.AddCell(p.Project.ProjectName);
                    table.AddCell(p.Deadline.ToString("dd/MM/yyyy"));
                    table.AddCell(p.Priority);
                    table.AddCell(p.Status);
                }

                // Add the table to the document
                document.Add(table);

                // Close the document
                document.Close();

                // Return the file as a PDF download
                return File(ms.ToArray(), "application/pdf", "Task Report.pdf");
            }
        }

        // Export to Excel (CSV)
        public IActionResult TaskExportToExcel()
        {
            var tasks = db.Task.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tasks");

                // Add header
                worksheet.Cells[1, 1].Value = "Task ID";
                worksheet.Cells[1, 2].Value = "Task Name";
                worksheet.Cells[1, 3].Value = "Project Name";
                worksheet.Cells[1, 4].Value = "Due Date";
                worksheet.Cells[1, 5].Value = "Priority";
                worksheet.Cells[1, 6].Value = "Status";

                // Add rows
                int row = 2;
                foreach (var p in tasks)
                {
                    worksheet.Cells[row, 1].Value = p.TaskId;
                    worksheet.Cells[row, 2].Value = p.Title;
                    worksheet.Cells[row, 3].Value = p.Project.ProjectName;
                    worksheet.Cells[row, 4].Value = p.Deadline.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 5].Value = p.Priority;
                    worksheet.Cells[row, 6].Value = p.Status;
                    row++;
                }

                // Save the file to a MemoryStream
                var fileBytes = package.GetAsByteArray();

                // Return the file as an Excel download
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Task Report.xlsx");
            }
        }

        public IActionResult FilteredTaskAction(string searchString, string status, int rowsPerPage = 10, int page = 1, string sortBy = "recent")
        {

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var query = db.Task.Where(x => x.TaskId == int.Parse(userId));
                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();

                    query = query.Where(x =>
                        (x.Status != null && x.Status.ToLower().Contains(searchString)) ||
                        (x.Deadline != null && x.Deadline.ToString("yyyy-MM-dd").Contains(searchString)) ||
                         (x.Title != null && x.Title.ToLower().Contains(searchString))

                         );
                    if (!string.IsNullOrEmpty(status) && status != "All")
                    {
                        query = query.Where(x => x.Status.ToLower() == status.ToLower());
                    }
                    switch (sortBy)
                    {
                        case "asc":
                            query = query.OrderBy(x => x.Deadline);
                            break;
                        case "desc":
                            query = query.OrderByDescending(x => x.Deadline); // Adjust column as needed
                            break;
                        case "recent":
                            query = query.OrderByDescending(x => x.Deadline); // Most recent first
                            break;
                        case "last7Days":
                            query = query.Where(x => x.Deadline >= DateTime.Now.AddDays(-7)).OrderByDescending(x => x.Deadline);
                            break;
                        case "lastMonth":
                            query = query.Where(x => x.Deadline >= DateTime.Now.AddMonths(-1)).OrderByDescending(x => x.Deadline);
                            break;
                        default:
                            query = query.OrderByDescending(x => x.Deadline); // Default sorting
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
    }
}

