using Microsoft.AspNetCore.Hosting.Server;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class ResignationController : Controller
    {
        private readonly ApplicationDbContext db;

        public ResignationController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // GET: Resignation/Index
        public IActionResult Index()
        {
            var resignations = (from r in db.Resignation
                                join u in db.User on r.UserID equals u.UserId
                                join d in db.Departments on r.DepartmentId equals d.DepartmentId
                                select new
                                {
                                    ResignationId = r.ResignationId,
                                    EmployeeName = u.FirstName + " " + u.LastName,
                                    ProfilePicture = u.ProfilePicture,
                                    DepartmentName = d.Name,
                                    Reason = r.Reason,
                                    NoticeDate = r.NoticeDate,
                                    ResignDate = r.ResignDate
                                }).ToList();

            return View(resignations);
        }

        // GET: Resignation/AddResignation
        public IActionResult AddResignation()
        {
            var users = db.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

            ViewBag.Users = new SelectList(users, "UserId", "FullName");
            //return PartialView("AddResignation");
            return View();
        }


        // POST: Resignation/AddResignation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddResignation(Resignation model)
        {
            var user = db.User.Find(model.UserID);
            var email = user?.Email;
            if (ModelState.IsValid)
            {
                if (true)
                {

                    if (user != null)
                    {
                        model.DepartmentId = user.DepartmentId ?? 0; // Assign user's department
                        db.Resignation.Add(model);
                        db.SaveChanges();

                        //ChangeStatus();

                        sendEmail(model, email);
                        TempData["success"] = "Resignation Added Successfully!!!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                return View(model);
            }
            else
            {
                var users = db.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

                ViewBag.Users = new SelectList(users, "UserId", "FullName");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["msg"] = "Fields should not be empty" + string.Join(", ", errors);
                return View();
            }

        }
        // GET: Resignation/EditResignation/5
        public IActionResult EditResignation(int id)
        {
            var resignation = db.Resignation
                .Include(r => r.User)
                .Include(r => r.Department)
                .FirstOrDefault(r => r.ResignationId == id);

            if (resignation == null)
            {
                return NotFound();
            }

            // Fetching active users and converting to SelectList
            ViewBag.Users = new SelectList(db.User
                .Where(u => u.Status == "Active")
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                }).ToList(), "UserId", "FullName");

            return PartialView("EditResignation", resignation);
        }

        // POST: Resignation/EditResignation
        [HttpPost]
        public IActionResult EditResignation(Resignation model)
        {
            //if (ModelState.IsValid)
            //{
            // Ensure the resignation entry is tracked and updated correctly
            db.Resignation.Update(model);
            db.SaveChanges();
            TempData["upd"] = "Resignation Updated Successfully!!";
            
            // Return JSON with redirect URL
            return RedirectToAction(nameof(Index));
            //}

            // If validation fails, return failure
            //return Json(new { success = false, message = "Validation failed. Please check the inputs." });
        }






        // POST: Resignation/DeleteResignation/5
        [HttpPost]
        public IActionResult DeleteResignation(int id)
        {
            var resignation = db.Resignation.Find(id);
            if (resignation != null)
            {
                db.Resignation.Remove(resignation);
                db.SaveChanges();
                TempData["error"] = "Resignation Deleted Successfully!!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
            
        }

        public void sendEmail(Resignation model, string email)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("dspatil1207@gmail.com");
            //mail.To.Add(TextBox1.Text);     //to send mail to one recipient

            mail.To.Add(email);    //to more recipients

            mail.Subject = "Resignation Acceptance Application";
            mail.Body = $"Dear user, \nYour Final resignation date is {model.ResignDate}";

            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("dspatil1207@gmail.com", "hfdvnlxbhixafwhi");
            smtp.Send(mail);
        }

        public void ChangeStatus()
        {
            // Get today's date
            var today = DateTime.Today;

            // Fetch terminations matching today's date
            var resignations = db.Resignation
                .Where(t => t.ResignDate == today)
                .ToList();

            foreach (var resignation in resignations)
            {
                // Get the associated user
                var user = db.User.FirstOrDefault(u => u.UserId == resignation.UserID);

                if (user != null && user.Status == "Active")
                {
                    // Update user status to 'Deactive'
                    user.Status = "Deactive";
                    db.SaveChanges();

                    // Send email notification
                    sendEmail(resignation, user.Email);

                    // Update termination status to 'Completed'
                    //termination.Status = "Completed";
                    db.SaveChanges();
                }
            }
        }

        [HttpGet]
        public IActionResult GetResignationData(string dateFilter)
        {
            DateTime today = DateTime.Today;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = today;

            switch (dateFilter)
            {
                case "today":
                    startDate = today;
                    endDate = today;
                    break;

                case "last_week":
                    startDate = today.AddDays(-7);
                    break;

                case "last_month":
                    startDate = today.AddMonths(-1);
                    break;

                case "last_year":
                    startDate = today.AddYears(-1);
                    break;

                case "all":
                default:
                    startDate = DateTime.MinValue;
                    break;
            }

            var resignations = db.Resignation
                .Where(r => r.ResignDate >= startDate && r.ResignDate <= endDate)
                .OrderBy(r => r.ResignDate)
                .Select(r => new
                {
                    r.ResignationId,
                    r.Reason,
                    EmployeeName = r.User.FirstName + " " + r.User.LastName,
                    r.Department.Name,
                    r.NoticeDate,
                    r.ResignDate
                })
                .ToList();

            return Json(resignations);
        }

    }
}