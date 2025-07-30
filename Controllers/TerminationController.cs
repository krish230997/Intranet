using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class TerminationController : Controller
    {
        private readonly ApplicationDbContext db;

        public TerminationController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddTermination()
        {
            var users = db.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

            ViewBag.Users = new SelectList(users, "UserId", "FullName");
            return PartialView("AddTermination");
        }

        [HttpPost]
        public IActionResult AddTermination(Termination termination)
        {
            var user = db.User.Find(termination.UserID);
            if (user == null) return Json(new { success = false, message = "User not found." });

            db.Termination.Add(termination);
            db.SaveChanges();
            ChangeStatus();

            return Json(new { success = true });
        }

        public IActionResult GetTerminations()
        {
            var result = (from r in db.Termination
                          join u in db.User on r.UserID equals u.UserId
                          select new
                          {
                    r.TerminationId,
                    EmployeeName = r.User != null ? r.User.FirstName + " " + r.User.LastName : "N/A",
                              ProfilePicture = u.ProfilePicture,
                              r.TerminationType,
                    NoticeDate = r.NoticeDate.ToString("yyyy-MM-dd"),
                    ResignDate = r.ResignDate.ToString("yyyy-MM-dd"),
                    r.Reason
                })
                .ToList();

            return Json(result);
        }

        [HttpPost]
        public IActionResult DeleteTermination(int id)
        {
            var termination = db.Termination.Find(id);
            if (termination == null)
                return Json(new { success = false, message = "Termination not found." });

            db.Termination.Remove(termination);
            db.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult EditTermination(int id)
        {
            var termination = db.Termination
                .Include(t => t.User)
                .FirstOrDefault(t => t.TerminationId == id);

            if (termination == null)
            {
                return Json(new { success = false, message = "Termination not found." });
            }

            var users = db.User
                .Select(u => new
                {
                    u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

            ViewBag.Users = new SelectList(users, "UserId", "FullName", termination.UserID);

            return PartialView("EditTermination", termination); // ✅ Ensure it returns the PartialView
        }

        [HttpPost]
        public IActionResult EditTermination(Termination termination)
        {
            var existingTermination = db.Termination.Find(termination.TerminationId);
            if (existingTermination == null)
            {
                return Json(new { success = false, message = "Termination not found." });
            }

            existingTermination.UserID = termination.UserID;
            existingTermination.TerminationType = termination.TerminationType;
            existingTermination.NoticeDate = termination.NoticeDate;
            existingTermination.ResignDate = termination.ResignDate;
            existingTermination.Reason = termination.Reason;

            db.SaveChanges();
            return Json(new { success = true });
        }

        public void ChangeStatus()
        {
            var today = DateTime.Today;
            var terminations = db.Termination
                .Where(t => t.ResignDate == today)
                .ToList();

            foreach (var termination in terminations)
            {
                var user = db.User.FirstOrDefault(u => u.UserId == termination.UserID);
                if (user != null && user.Status == "Active")
                {
                    user.Status = "Deactive";
                    db.SaveChanges();
                }
            }
        }
    }
}