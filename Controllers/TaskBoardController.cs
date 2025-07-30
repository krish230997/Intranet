using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.Linq;

namespace Pulse360.Controllers
{
    public class TaskBoardController : Controller
    {
        private readonly ApplicationDbContext db;

        public TaskBoardController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index(string searchQuery)
        {
            var projectQuery = db.AllProjects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.TaskBoards)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Taskmember)
                .Include(u => u.Users)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                projectQuery = projectQuery.Where(p => p.ProjectName.ToLower().Contains(searchQuery.ToLower()));
            }

            var projectsWithTasks = projectQuery.Select(p => new
            {
                ProjectName = p.ProjectName,
                TotalTasks = p.Tasks.Count(),
                PendingTasks = p.Tasks.Count(t => t.Status == "Pending"),
                CompletedTasks = p.Tasks.Count(t => t.Status == "Completed"),
                Tasks = p.Tasks.Select(t => new
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Status = t.Status.Trim(),
                    Priority = t.Priority,

                    // Fetch Percentage directly from TaskBoards where TaskId matches
                    Percentage = db.TaskBoards
                        .Where(tb => tb.TaskId == t.TaskId)
                        .Select(tb => tb.Percentage)
                        .FirstOrDefault(),

                    // Fetch DueDate directly from TaskBoards where TaskId matches
                    DueDate = db.TaskBoards
                        .Where(tb => tb.TaskId == t.TaskId)
                        .Select(tb => (DateTime?)tb.DueDate)
                        .FirstOrDefault() ?? t.Deadline,

                    Members = t.Taskmember
                        .Select(tm => tm.User.ProfilePicture) // Fetch ProfilePicture from related User table
                        .ToList()
                }).ToList()
            }).ToList();

            ViewBag.Statuses = new List<string> { "Pending", "Inprogress", "Onhold", "Completed" };
            return View(projectsWithTasks);
        }
    }
}