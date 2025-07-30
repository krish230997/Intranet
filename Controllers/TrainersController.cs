using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext db;

        public TrainersController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult AddTrainer()
        {
            // Load roles into ViewBag
            ViewBag.Roles = db.Role.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult AddTrainer(Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in the database
                if (db.Trainer.Any(t => t.Email == trainer.Email))
                {
                    ModelState.AddModelError("Email", "This email is already in use.");
                    ViewBag.Roles = db.Role.ToList();
                    return View(trainer);
                }

                if (trainer.imagename != null && trainer.imagename.Length > 0)
                {
                    // Validate file extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(trainer.imagename.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("imagename", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        ViewBag.Roles = db.Role.ToList(); // Reload roles before returning view
                        return View(trainer);
                    }

                    // Save file
                    string fileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        trainer.imagename.CopyTo(stream);
                    }

                    trainer.ProfilePicture = fileName; // Save filename in DB
                }

                db.Trainer.Add(trainer);
                db.SaveChanges();
                TempData["msg"] = "Trainer Added Successfully!!";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = db.Role.ToList(); // Reload roles before returning view
            return View(trainer);
        }



        public IActionResult EditTrainer(int id)
        {
            var trainer = db.Trainer.Find(id);
            if (trainer == null)
            {
                return NotFound();
            }

            // Get the list of rolesq
            ViewBag.Roles = db.Role.ToList();

            return View(trainer);
        }

        public IActionResult DeleteTrainer(int id)
        {
            var did=db.Trainer.Find(id);
            if(did!=null)
            {
                db.Trainer.Remove(did);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult EditTrainer(Trainer trainer, IFormFile? imagename)
        {
            if (ModelState.IsValid)
            {
                var existingTrainer = db.Trainer.Find(trainer.TrainerId);
                if (existingTrainer == null)
                {
                    return NotFound();
                }

                // Check if the new email is already in use by another trainer
                if (db.Trainer.Any(t => t.Email == trainer.Email && t.TrainerId != trainer.TrainerId))
                {
                    ModelState.AddModelError("Email", "This email is already in use by another trainer.");
                    ViewBag.Roles = db.Role.ToList();
                    return View(trainer);
                }

                // Update trainer details except ProfilePicture
                existingTrainer.FirstName = trainer.FirstName;
                existingTrainer.LastName = trainer.LastName;
                existingTrainer.Email = trainer.Email;
                existingTrainer.Description = trainer.Description;
                existingTrainer.Status = trainer.Status;
                existingTrainer.Role = trainer.Role;

                // If a new image is uploaded, validate and update
                if (imagename != null && imagename.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(imagename.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("imagename", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        ViewBag.Roles = db.Role.ToList();
                        return View(trainer);  // Return the view instead of refreshing the page
                    }

                    // Generate unique file name
                    string fileName = Guid.NewGuid().ToString() + extension;
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        imagename.CopyTo(stream);
                    }

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existingTrainer.ProfilePicture))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "uploads", existingTrainer.ProfilePicture);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save the new image file name
                    existingTrainer.ProfilePicture = fileName;
                }

                db.SaveChanges();
                TempData["msg"] = "Trainer updated successfully!";
                return RedirectToAction("Index");
            }

            // Reload roles if validation fails
            ViewBag.Roles = db.Role.ToList();
            return View(trainer);
        }

        // GET: Trainers/Index (For listing all trainers)

        public IActionResult Index(string searchString)
        {
            // Fetch all trainers initially
            var trainers = db.Trainer.AsQueryable();

            // Apply search filter if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                trainers = trainers.Where(t =>
                    t.FirstName.Contains(searchString) ||
                    t.LastName.Contains(searchString) ||
                    t.Email.Contains(searchString) ||
                    t.Role.Contains(searchString) ||
                    t.Status.Contains(searchString)
                );
            }

            // Pass the search string to the view for retaining it in the search box
            ViewData["SearchString"] = searchString;

            return View(trainers.ToList());
        }
        //public IActionResult Index()
        //{
        //    var trainers = db.Trainer.ToList();
        //    return View(trainers);
        //}

    }
}
