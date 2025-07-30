using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;

namespace Pulse360.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;
        public AdminController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult EmpByDeptGraph()
        {
            return View();
        }

        public IActionResult EmpByDeptGraphData()
        {
            var data = db.User
                .GroupBy(u => u.Department.Name)
                .Select(g => new
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count()
                })
                .ToList();

            return Json(data);
        }

        public IActionResult TasksStats()
        {
            // Total tasks
            //int totalTasks = db.Tasks.Count();

            //if (totalTasks == 0)
            //{
            //    ViewBag.InProgress = 0;
            //    ViewBag.OnHold = 0;
            //    ViewBag.Overdue = 0;
            //    ViewBag.Completed = 0;
            //    ViewBag.TotalTasks = "0/0";
            //    ViewBag.InProgressPercent = 0;
            //    ViewBag.OnHoldPercent = 0;
            //    ViewBag.OverduePercent = 0;
            //    ViewBag.CompletedPercent = 0;
            //    return View();
            //}

            //// Task counts
            //int inProgressCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "inprogress");
            //int onHoldCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "onhold");
            //int overdueCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "overdue");
            //int completedCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "completed");

            //ViewBag.InProgress = inProgressCount;
            //ViewBag.OnHold = onHoldCount;
            //ViewBag.Overdue = overdueCount;
            //ViewBag.Completed = completedCount;
            //ViewBag.TotalTasks = $"{completedCount}/{totalTasks}";
            //int total = (int)totalTasks;

            //// Calculate percentages
            //ViewBag.InProgressPercent = (inProgressCount * 100) / totalTasks;
            //ViewBag.OnHoldPercent = (onHoldCount * 100) / totalTasks;
            //ViewBag.OverduePercent = (overdueCount * 100) / totalTasks;
            //ViewBag.CompletedPercent = (completedCount * 100) / totalTasks;

            return View();
        }

    }
}
