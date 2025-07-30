using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class TrainingTypeController : Controller
    {
        private readonly ApplicationDbContext db;

        public TrainingTypeController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // GET: /TrainingType/Index
        public IActionResult Index()
        {
            var data = db.TrainingType.ToList();
            return View(data);
        }

        // GET: /TrainingType/AddTrainingType
        public IActionResult AddTrainingType()
        {
            return View();
        }

        // POST: /TrainingType/AddTrainingType
        [HttpPost]
        public IActionResult AddTrainingType(TrainingType trainingType)
        {
            if (ModelState.IsValid)
            {
                db.TrainingType.Add(trainingType);
                db.SaveChanges();
                TempData["msg"] = "Training Type added successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                // If the model is not valid, return to the AddTrainingType view
                return View();
            }

               
        }

        // Delete action
        public IActionResult DeletetrainingType(int id)
        {
            var data = db.TrainingType.Find(id);
            if (data != null)
            {
                db.TrainingType.Remove(data);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return null;
            }
        }

        // Edit action
        public IActionResult Edit(int id)
        {
            var masterEvent = db.TrainingType.Find(id);
            return View(masterEvent);
        }

        [HttpPost]
        public IActionResult Edit(TrainingType Training)
        {
            if (ModelState.IsValid)
            {
                db.TrainingType.Update(Training);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(Training);
        }
    }
}
