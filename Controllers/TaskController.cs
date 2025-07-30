using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
//using Pulse360.Migrations;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class TaskController : Controller
    {
		private readonly ApplicationDbContext db;

		public TaskController(ApplicationDbContext db)
		{
			this.db = db;
		}
        //public IActionResult Index()
        //{


        //	var projects = db.AllProjects
        //  .Include(p => p.Tasks)
        //  .ThenInclude(t => t.TaskBoards)
        //  .ToList()
        //  .Select(p => {
        //   var totalTasks = p.Tasks.Count();
        //   var completedTasks = p.Tasks.Count(t => t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
        //   var percentage = totalTasks > 0 ? (completedTasks * 100.0) / totalTasks : 0; // Avoid division by zero

        //   return new
        //   {
        //	   ProjectName = p.ProjectName,
        //	   Deadline = p.EndDate,
        //	   Value = p.ProjectValue,
        //	   TotalHours = (p.EndDate - p.StartDate).TotalHours,
        //	   TotalTasks = totalTasks,
        //	   CompletedTasks = completedTasks,
        //	   Percentage = Math.Round(percentage, 2), // Rounded to 2 decimal places
        //	   ManagerName = p.ManagerName,
        //	   LogoPath = p.LogoPath.TrimStart('/', '\\')
        //   };
        //  })
        //  .ToList();

        //	return View(projects);
        //}

        public IActionResult Index(string priority = "All", DateTime? dueDate = null)
        {
            try
            {
                // Log parameters for debugging
                Console.WriteLine($"Received priority: {priority}, dueDate: {dueDate}");

                var projects = db.AllProjects.Where(x=>x.Status.Equals("Active"))
                    .Include(p => p.Tasks)
                    .ThenInclude(t => t.TaskBoards)
                    .ToList()
                    .Select(p =>
                    {
                        var totalTasks = p.Tasks?.Count() ?? 0;
                        var completedTasks = p.Tasks?.Count(t => t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)) ?? 0;
                        var percentage = totalTasks > 0 ? (completedTasks * 100.0) / totalTasks : 0;

                        return new
                        {
                            ProjectName = p.ProjectName,
                            Deadline = p.EndDate != default ? p.EndDate : DateTime.Now,  // Default to current date if missing
                            Value = p.ProjectValue,
                            TotalHours = (p.EndDate != default && p.StartDate != default) ?
                 (p.EndDate - p.StartDate).TotalHours : 0,  // Calculate only if both dates exist
                            TotalTasks = totalTasks,
                            CompletedTasks = completedTasks,
                            Percentage = Math.Round(percentage, 2),
                            ManagerName = p.ManagerName,
                            LogoPath = !string.IsNullOrEmpty(p.LogoPath) ? p.LogoPath.TrimStart('/', '\\') : "",
                            Priority = !string.IsNullOrEmpty(p.Priority) ? p.Priority : "Medium" // Default if Priority is missing
                        };
                    })
                    .ToList();

                // **Filtering Logic**
                if (!string.IsNullOrEmpty(priority) && priority != "All")
                {
                    projects = projects.Where(p => p.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (dueDate.HasValue)
                {
                    projects = projects.Where(p => p.Deadline.Date == dueDate.Value.Date).ToList();
                }

                return View(projects);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Index: {ex.Message}");
                return View("Error");
            }
        }




        public IActionResult GetProjectMembers(int projectId)
        {
            if (projectId == 0)
            {
                return Json(new { success = false, message = "Invalid Project ID" });
            }

            // Fetch project members using navigation properties
            var members = db.AllProjects
                .Where(p => p.ProjectId == projectId)
                .SelectMany(p => p.Users) // Assuming the navigation property is `Users`
                .Select(u => new
                {
                    userId = u.UserId,
                    fullName = u.FirstName + " " + u.LastName
                })
                .Distinct()
                .ToList();

            if (!members.Any())
            {
                return Json(new { success = false, message = "No members found" });
            }

            return Json(new { success = true, data = members });
        }

        public IActionResult Task()
        {
            ViewBag.Projects = db.AllProjects.Where(x=>x.Status.Equals("Active"))
            .Select(p => new { p.ProjectId, p.ProjectName })
            .Distinct()
            .ToList();

            ViewBag.Members = new List<object>(); // Initially empty, will be populated based on project selection.

            return View();


        }

        //[HttpPost]
        //public IActionResult AddMember(string name)
        //{
        //	if (!string.IsNullOrEmpty(name))
        //	{
        //		return Json(new { success = true, name });
        //	}
        //	return Json(new { success = false });
        //}

        [HttpPost]
		public IActionResult Task(Tasks task, IFormFile file, List<int> AssignedUserIds)
		{
			//if(ModelState.IsValid)
			//{
                var username = User.Identity.Name;
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                task.FilePath = $"/uploads/{uniqueFileName}";


                db.Task.Add(task);
                db.SaveChanges();

                if (AssignedUserIds != null && AssignedUserIds.Any())
                {
                    foreach (var userId in AssignedUserIds)
                    {
                        db.Taskmember.Add(new TaskMembers
                        {
                            TaskId = task.TaskId,
                            UserId = userId
                        });
                    }
                    db.SaveChanges();
                }
            TempData["success"] = "Task Added Successfully!!";
                return RedirectToAction("Index");
   //         }
			//else
			//{
   //             ViewBag.Projects = db.AllProjects
			//	.Select(p => new { p.ProjectId, p.ProjectName })
			//	.Distinct()
			//	.ToList();

   //             ViewBag.Members = db.User
   //                 .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName })
   //                 .ToList();

			//	var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
			//	TempData["msg"] = "Fields should not be empty" + string.Join(", ", errors);
			//	//TempData["msg"] = "Fields Should be not Empty!!";
			//	return View();
			//}



		}

		[HttpGet]
		public IActionResult Edit(int taskId)
		{
			var task = db.Task
						 .Include(t => t.Taskmember)
						 .ThenInclude(tm => tm.User) // Include related users
						 .FirstOrDefault(t => t.TaskId == taskId);

			if (task == null)
			{
				return NotFound();
			}

			// Populate a list of user IDs currently assigned to the task
			var assignedUserIds = task.Taskmember.Select(tm => tm.UserId).ToList();

			ViewBag.AssignedUserIds = assignedUserIds;
			ViewBag.AllUsers = db.User
				.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName })
				.ToList(); // Assuming you have a Users table

			return View(task);
		}

		[HttpPost]
		public IActionResult Edit(int taskId, Tasks task, IFormFile file, List<int> AssignedUserIds)
		{
			if (AssignedUserIds == null || !AssignedUserIds.Any())
			{
				Console.WriteLine("AssignedUserIds is empty or null!");
			}
			else
			{
				Console.WriteLine($"Assigned Users Count: {AssignedUserIds.Count}");
			}

			var existingTask = db.Task.Include(t => t.Taskmember)
									  .FirstOrDefault(t => t.TaskId == taskId);

			if (existingTask == null)
			{
				return NotFound();
			}

			// Update fields
			existingTask.Title = task.Title;
			existingTask.Description = task.Description;
			existingTask.Deadline = task.Deadline;
			existingTask.Priority = task.Priority;

			// File upload
			if (file != null)
			{
				string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder);
				}

				string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					file.CopyTo(stream);
				}

				existingTask.FilePath = $"/uploads/{uniqueFileName}";
			}

			// Clear previous user assignments
			existingTask.Taskmember.Clear();

			// Add new assigned users
			if (AssignedUserIds != null && AssignedUserIds.Any())
			{
				foreach (var userId in AssignedUserIds)
				{
					existingTask.Taskmember.Add(new TaskMembers
					{
						TaskId = existingTask.TaskId,
						UserId = userId
					});
				}
			}

			db.SaveChanges();
			TempData["upd"] = "Task Updated Successfully!!!";
			return RedirectToAction("Index", "TaskBoard");
		}
		//public IActionResult Delete(int taskId)
		//{
		//    var task = db.Task.Find(taskId);
		//    if (task != null)
		//    {
		//        db.Task.Remove(task);
		//        db.SaveChanges();
		//    }

		//    return RedirectToAction("Index", "TaskBoard");
		//}

		public IActionResult Delete(int taskId)
		{
			var task = db.Task
						 .Include(t => t.Taskmember)
						 .Include(t => t.TaskBoards) // Include related TaskBoards
						 .FirstOrDefault(t => t.TaskId == taskId);

			if (task == null)
			{
				return NotFound();
			}

			// Delete related TaskBoards records
			var relatedTaskBoards = db.TaskBoards.Where(tb => tb.TaskId == taskId).ToList();
			if (relatedTaskBoards.Any())
			{
				db.TaskBoards.RemoveRange(relatedTaskBoards);
			}

			// Delete related TaskMembers records
			var relatedTaskMembers = db.Taskmember.Where(tm => tm.TaskId == taskId).ToList();
			if (relatedTaskMembers.Any())
			{
				db.Taskmember.RemoveRange(relatedTaskMembers);
			}

			// Remove the uploaded file if it exists
			if (!string.IsNullOrEmpty(task.FilePath))
			{
				string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", task.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}

			// Now, delete the Task
			db.Task.Remove(task);
			db.SaveChanges();
			TempData["error"] = "Task Deleted Successfully!!";
			return RedirectToAction("Index", "TaskBoard");
		}



	}
}
