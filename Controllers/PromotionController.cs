using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.Linq;

namespace Pulse360.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext db;

        public PromotionController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddPromotion()
        {
            // Fetch users and designations for dropdowns
            var users = db.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

            var designations = db.Designations
                .Select(d => new
                {
                    d.Name
                })
                .ToList();

            ViewBag.Users = new SelectList(users, "UserId", "FullName");
            ViewBag.Designations = new SelectList(designations, "Name", "Name");

            return PartialView("AddPromotion");
        }

        [HttpPost]
        public IActionResult AddPromotion(Promotion promotion)
        {
            var user = db.User.Find(promotion.UserID);

            // Add promotion record
            db.Promotion.Add(promotion);
            db.SaveChanges();

            ChangeDesignation();

            return Json(new { success = true });
        }

        public IActionResult GetPromotions()
        {
            var result = (from r in db.Promotion
                         join u in db.User on r.UserID equals u.UserId

                         select new
                         {
                             ProfilePicture = u.ProfilePicture,
                             r.PromotionId,
                    EmployeeName = r.User != null ? r.User.FirstName + " " + r.User.LastName : "N/A",
                    r.DesignationFrom,
                    r.DesignationTo,
                    PromotionDate = r.Date.ToString("yyyy-MM-dd")
                         }).ToList();


            return Json(result);
        }

        [HttpPost]
        public IActionResult DeletePromotion(int id)
        {
            var promotion = db.Promotion.Find(id);
            if (promotion == null)
                return Json(new { success = false, message = "Promotion not found." });

            db.Promotion.Remove(promotion);
            db.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult EditPromotion(int id)
        {
            var promotion = db.Promotion
                .Include(t => t.User)
                .FirstOrDefault(t => t.PromotionId == id);

            if (promotion == null)
            {
                return NotFound();
            }

            // Fetch users for dropdown
            var users = db.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

            var designations = db.Designations
                .Select(d => new
                {
                    d.Name
                })
                .ToList();

            // Set the selected values correctly
            ViewBag.Users = new SelectList(users, "UserId", "FullName", promotion.UserID);
            ViewBag.DesignationsFrom = new SelectList(designations, "Name", "Name", promotion.DesignationFrom);
            ViewBag.DesignationsTo = new SelectList(designations, "Name", "Name", promotion.DesignationTo);
            return PartialView("EditPromotion", promotion);
        }

        [HttpPost]
        public IActionResult EditPromotion(Promotion promotion)
        {

            var existingPromotion = db.Promotion.Find(promotion.PromotionId);
            if (existingPromotion == null)
            {
                return Json(new { success = false, message = "Promotion not found." });
            }

            existingPromotion.UserID = promotion.UserID;
            existingPromotion.DesignationFrom = promotion.DesignationFrom;
            existingPromotion.DesignationTo = promotion.DesignationTo;
            existingPromotion.Date = promotion.Date;

            db.SaveChanges();
            return Json(new { success = true });
        }

        public void sendEmail(Promotion model, string email)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("dspatil1207@gmail.com");
            mail.To.Add(email);
            mail.Subject = "Congratulations on Your Promotion!";
            mail.Body = $"Dear {model.User.FirstName},\n\n" +
                         $"Congratulations on your promotion to the {model.DesignationTo} position!\n" +
                        $"Your new designation is now effective from {model.Date.ToShortDateString()}.\n\n" +
                        "Best regards,\nYour Company Team";

            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("dspatil1207@gmail.com", "hfdvnlxbhixafwhi");
            smtp.Send(mail);
        }

        public void ChangeDesignation()
        {
            // Get today's date
            var today = DateTime.Today;

            // Fetch promotions that match today's date or are scheduled in the future
            var promotions = db.Promotion
                .Where(p => p.Date.Date == today)  // Match today's date
                .ToList();

            foreach (var promotion in promotions)
            {
                // Get the associated user (employee)
                var user = db.User.FirstOrDefault(u => u.UserId == promotion.UserID);

                if (user != null)
                {
                    // Check if the user is not already at the target designation


                    // Update the user's designation after promotion
                    var newDesignation = db.Designations.FirstOrDefault(d => d.Name == promotion.DesignationTo);
                    if (newDesignation != null)
                    {
                        user.DesignationtId = newDesignation.DesignationId;  // Update designation to the new one
                        db.SaveChanges();  // Save the changes to the user

                        // Optionally update the user status or other fields as needed (e.g., marking as promoted)
                        // user.Status = "Promoted"; // For example, update user status if required
                        // db.SaveChanges(); // Save changes if status is modified


                        // Send email notification about the promotion
                        sendEmail(promotion, user.Email);
                    }
                }
            }
        }

    }
}