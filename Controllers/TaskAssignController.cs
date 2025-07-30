using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class TaskAssignController : Controller
    {
        private readonly ApplicationDbContext db;

        public TaskAssignController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index(Projects p)
        {
            ViewBag.Projects = db.AllProjects
       .Select(p => new { p.ProjectId, p.ProjectName })
       .Distinct()
       .ToList();
            if (ViewBag.Projects != null)
            {
                var data = (from r in db.AllProjects
                            join u in db.Task on r.ProjectId equals u.ProjectId
                            join d in db.TaskBoards on r.ProjectId equals d.ProjectId
                            where r.ProjectName == p.ProjectName
                            select new
                            {
                                ProjectName = r.ProjectName,
                                Status = u.Status,
                                Priority = u.Priority,
                                Title = u.Title,
                                Percentage = d.Percentage,
                                DueDate = d.DueDate,


                            }).ToList();
                return View(data);
            }

            return View();

        }
        //public IActionResult TaskBoard()
        //{
        //    ViewBag.Projects = db.AllProjects.Select(p => new { p.ProjectId, p.ProjectName }).ToList();
        //    ViewBag.Tasks = db.Task.Where(x => x.ProjectId == x.TaskId).Select(t => new { t.TaskId, t.Title }).ToList();
        //    return View();
        //}


        public IActionResult TaskBoard()
        {
            ViewBag.Projects = db.AllProjects.Where(x => x.Status.Equals("Active")).Select(p => new { p.ProjectId, p.ProjectName }).ToList();
            ViewBag.Tasks = new List<object>(); // Empty initially, will be loaded via AJAX
            return View();
        }

        // API Endpoint to Fetch Tasks by ProjectId
        public JsonResult GetTasksByProject(int projectId)
        {
            var tasks = db.Task
                .Where(t => t.ProjectId == projectId)
                .Select(t => new { t.TaskId, t.Title })
                .ToList();

            return Json(tasks);
        }



        [HttpPost]
        public IActionResult TaskBoard(TaskBoards model)
        {

            db.TaskBoards.Add(model);
            db.SaveChanges();
            TempData["success"] = "Task saved successfully!";
            return RedirectToAction("Index","TaskBoard");

            //ViewBag.Projects = db.AllProjects.Select(p => new { p.ProjectId, p.ProjectName }).ToList();
            //ViewBag.Tasks = db.Task.Select(t => new { t.TaskId, t.Title }).ToList();
            //return View("TaskBoard", model);
        }

    }
}
