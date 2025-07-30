using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class AddTrainingController : Controller
    {
        private readonly ApplicationDbContext db;

        public AddTrainingController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            var Users = db.User.Where(u => u.Status == "Active").ToList();
            var Trainers = db.Trainer.Where(t => t.Status == "Active").ToList();
            var trainings = db.Training
                                .Include(t => t.Trainer)
                                .Include(t => t.TrainingType)
                                .Include(t => t.User)
                                .ToList();
            trainings.ForEach(x => x.ProfilePicture = Trainers.FirstOrDefault(y => y.TrainerId == x.TrainerId).ProfilePicture);

            trainings.ForEach(x => x.images.Add(Users.FirstOrDefault(y => y.UserId == x.UserId).ProfilePicture));

            //foreach(var item in trainings)
            //{
            //    trainings.Where(x=>x.images.AddRange())
            //}
            return View(trainings);
        }



        [HttpGet]
        public IActionResult AddTraining()
        {
            ViewBag.Trainers = db.Trainer.Where(t => t.Status == "Active").ToList();
            ViewBag.TrainingTypes = db.TrainingType.Where(tt => tt.Status == "Active").ToList();
            ViewBag.Users = db.User.Where(u => u.Status == "Active").ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddTraining(AddTrainingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = User.Identity.Name ?? "System"; // Fetch logged-in user's name

                var training = new Pulse360.Models.Training
                {
                    TrainerId = model.TrainerId,
                    TrainingTypeId = model.TrainingTypeId,
                    UserId = model.UserId,
                    TrainingCost = model.TrainingCost,
                    Description = model.Description,
                    Status = model.Status,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,

                    // New fields
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUser,
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = currentUser
                };

               

                db.Training.Add(training);
                db.SaveChanges();
                TempData["msg"] = "Training added successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Trainers = db.Trainer.Where(t => t.Status == "Active").ToList();
                ViewBag.TrainingTypes = db.TrainingType.Where(tt => tt.Status == "Active").ToList();
                ViewBag.Users = db.User.Where(u => u.Status == "Active").ToList();

                TempData["msg"] = "All Fields Required!";
                return View();
            }



                //return View(model);
        }

        public IActionResult DeleteTraining(int id)
        {
            var data = db.Training.Find(id);
            if (data != null)
            {
                db.Training.Remove(data);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditTraining(int id)
        {
            var training = db.Training
                                .Include(t => t.Trainer)
                                .Include(t => t.TrainingType)
                                .Include(t => t.User)
                                .FirstOrDefault(t => t.TrainingId == id);

            if (training == null)
            {
                return NotFound();
            }

            var viewModel = new AddTrainingViewModel
            {
                TrainerId = training.TrainerId,
                TrainingTypeId = training.TrainingTypeId,
                UserId = training.UserId,
                TrainingCost = training.TrainingCost,
                Description = training.Description,
                Status = training.Status,
                StartDate = training.StartDate,
                EndDate = training.EndDate
            };

            // Populate dropdown lists using ViewBag
            ViewBag.Trainers = db.Trainer.Select(t => new { t.TrainerId, t.FirstName }).ToList();
            ViewBag.TrainingTypes = db.TrainingType.Select(tt => new { tt.TrainingTypeId, tt.TrainingTypeName }).ToList();
            ViewBag.Users = db.User.Select(u => new { u.UserId, u.FirstName }).ToList();

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTraining(int id, AddTrainingViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var training = db.Training.FirstOrDefault(t => t.TrainingId == id);
                if (training == null)
                {
                    return NotFound();
                }

                try
                {
                    var currentUser = User.Identity.Name ?? "System"; // Get logged-in user

                    // Update fields
                    training.TrainerId = viewModel.TrainerId;
                    training.TrainingTypeId = viewModel.TrainingTypeId;
                    training.UserId = viewModel.UserId;
                    training.TrainingCost = viewModel.TrainingCost;
                    training.Description = viewModel.Description;
                    training.Status = viewModel.Status;
                    training.StartDate = viewModel.StartDate;
                    training.EndDate = viewModel.EndDate;

                    // Update modified details
                    training.ModifiedAt = DateTime.Now;
                    training.ModifiedBy = currentUser;

                    db.SaveChanges();
                    TempData["msg"] = "Training updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again later.");
                }
            }

            ViewBag.Trainers = db.Trainer.Select(t => new { t.TrainerId, t.FirstName }).ToList();
            ViewBag.TrainingTypes = db.TrainingType.Select(tt => new { tt.TrainingTypeId, tt.TrainingTypeName }).ToList();
            ViewBag.Users = db.User.Select(u => new { u.UserId, u.FirstName }).ToList();

            return View(viewModel);
        }


    }
}