using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class GoalController : Controller
    {
        private readonly ApplicationDbContext db;
        public GoalController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index(string searchString, string sortBy = "recent")
        {
            ViewData["SearchString"] = searchString;
            ViewBag.SortBy = sortBy; // Use ViewBag instead of ViewData for simpler access

            var data = db.GoalTypeList.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(g => g.GoalType.Contains(searchString) ||
                                       g.Description.Contains(searchString) ||
                                       g.Status.Contains(searchString));
            }

            switch (sortBy?.ToLower())
            {
                case "asc":
                    data = data.OrderBy(g => g.GoalId);
                    break;
                case "desc":
                    data = data.OrderByDescending(g => g.GoalId);
                    break;
                case "recent":
                default:
                    data = data.OrderByDescending(g => g.GoalId);
                    break;
            }

            return View(data.ToList());

        }

        public IActionResult AddGoal()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddGoal(GoalTypeList g)
        {

            if(ModelState.IsValid)
            {
                db.GoalTypeList.Add(g);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["msg"] = "Fields Should be not Empty!!";
                return View();
            }


        }

        public IActionResult EditGoalType(int id)
        {
            var goalType = db.GoalTypeList.Find(id);
            if (goalType == null)
            {
                return NotFound("Goal Type not found.");
            }
            return View(goalType);
        }

        [HttpPost]
        public IActionResult EditGoalType(GoalTypeList pa)
        {


            var existingAppriasal = db.GoalTypeList.Find(pa.GoalId);
            if (existingAppriasal == null)
            {
                return NotFound("Goal Type not found.");
            }
            else
            {
                existingAppriasal.GoalId = pa.GoalId;
                existingAppriasal.GoalType = pa.GoalType;
                existingAppriasal.Description = pa.Description;

                existingAppriasal.Status = pa.Status;
                db.GoalTypeList.Update(existingAppriasal);
                db.SaveChanges();
                return RedirectToAction("Index"); // Ensure this is the correct action
            }
                
        }

        [HttpPost]
        public IActionResult DeleteGoalType(int id)
        {
            var goalType = db.GoalTypeList.Find(id);

            if (goalType == null)
            {
                return NotFound("Goal Type not found.");
            }
            else
            {
                db.GoalTypeList.Remove(goalType);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            
        }


        public IActionResult Fetch(string searchString, string sortBy = "recent")
        {
            ViewData["SearchString"] = searchString;
            ViewBag.SortBy = sortBy;


            var data = db.GoalTrackingList.Include(x => x.GoalTypeList).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(g => g.Description.Contains(searchString) ||
                                       g.Subject.Contains(searchString) ||
                                       g.Status.Contains(searchString) ||
                                       g.TargetAchievement.Contains(searchString));
            }


            switch (sortBy)
            {
                case "asc":
                    data = data.OrderBy(g => g.Subject);
                    break;
                case "desc":
                    data = data.OrderByDescending(g => g.Subject);
                    break;
                case "recent":
                default:
                    data = data.OrderByDescending(g => g.StartDate);
                    break;
            }

            return View(data.ToList());
        }




        public IActionResult AddGoalTracking()
        {
            var goalTypes = db.GoalTypeList.Where(x=>x.Status.Equals("Active")).ToList();
            ViewBag.goal = new SelectList(goalTypes, "GoalId", "GoalType");
            return View();
        }

        [HttpPost]
        public IActionResult AddGoalTracking(GoalTrackingList g)
        {
            if (ModelState.IsValid)
            {
                db.GoalTrackingList.Add(g);
                db.SaveChanges();
                return RedirectToAction("Fetch");
            }
            else
            {
                // Retrieve ModelState errors for debugging (optional)
                //var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                //TempData["msg"] = "Fields should not be empty" + string.Join(", ", errors);
                TempData["msg"] = "Fields should not be empty";
                // Repopulate the dropdown list
                ViewBag.goal = new SelectList(db.GoalTypeList.Where(x=>x.Status.Equals("Active")), "GoalId", "GoalType");
                return View(g);  // Return the model to repopulate the form
            }


        }
        public IActionResult EditGoalTracking(int id)
        {
            ViewBag.goal = new SelectList(db.GoalTypeList, "GoalId", "GoalType");

            var apprisal = db.GoalTrackingList.Find(id);
            if (apprisal == null)
            {
                return NotFound();
            }
            return View(apprisal);
        }

        [HttpPost]
        public IActionResult EditGoalTracking(GoalTrackingList pa)
        {


            var existingGoalTracking = db.GoalTrackingList.Find(pa.GoalTrackingId);
            if (existingGoalTracking == null)
            {
                return NotFound("Goal Tracking not found.");
            }

            else
            {
                existingGoalTracking.Subject = pa.Subject;
                existingGoalTracking.TargetAchievement = pa.TargetAchievement;
                existingGoalTracking.GoalId = pa.GoalId;
                existingGoalTracking.StartDate = pa.StartDate;
                existingGoalTracking.EndDate = pa.EndDate;
                existingGoalTracking.Description = pa.Description;
                existingGoalTracking.Status = pa.Status;
                db.Update(existingGoalTracking);
                db.SaveChanges();
                return RedirectToAction("Fetch"); // Ensure this is the correct action
            }
                // Update properties
           
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var performanceApprisal = db.GoalTrackingList.Find(id);

            if (performanceApprisal == null)
            {
                return NotFound();
            }
            else
            {
                db.GoalTrackingList.Remove(performanceApprisal);
                db.SaveChanges();
                return RedirectToAction("Fetch");
            }

            
        }
    }
}
