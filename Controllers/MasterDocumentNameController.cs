using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class MasterDocumentNameController : Controller
    {
        private readonly ApplicationDbContext db;

        public MasterDocumentNameController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddDocNameAdmin()
        {

            return View();
        }
        [HttpPost]
        public IActionResult AddDocNameAdmin(AddAdminDocName ad)
        {
            if (ModelState.IsValid)
            {
                db.addAdminDocNames.Add(ad);
                db.SaveChanges();
                TempData["DocAdded"] = "Admin Document Name added successfully";
                return RedirectToAction("FetchAdminDocName");
            }

            // Return the same view with the model to show validation messages
            return View(ad);
        }

        public IActionResult AddDocNameEmployee()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddDocNameEmployee(AddEmployeeDocName ae)
        {
            if (ModelState.IsValid)
            {
                db.addEmployeeDocNames.Add(ae);
                db.SaveChanges();
                TempData["DocAdded"] = "Employee Document Name added successfully";
                return RedirectToAction("FetchEmployeeDocName");
            }
            return View(ae);
        }

        public IActionResult FetchAdminDocName()
        {
            var data = db.addAdminDocNames.ToList();
            return View(data);
        }
        public IActionResult FetchEmployeeDocName()
        {
            var data = db.addEmployeeDocNames.ToList();
            return View(data);
        }

        public IActionResult EditAdminDocName(int id)
        {
            var data = db.addAdminDocNames.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult EditAdminDocName(AddAdminDocName ud)
        {
            db.addAdminDocNames.Update(ud);
            db.SaveChanges();
            return RedirectToAction("FetchAdminDocName");
        }

        public IActionResult EditEmployeeDocName(int id)
        {
            var data = db.addEmployeeDocNames.Find(id);
            return View(data);
        }

        [HttpPost]
        public IActionResult EditEmployeeDocName(AddEmployeeDocName ud)
        {
            db.addEmployeeDocNames.Update(ud);
            db.SaveChanges();
            return RedirectToAction("FetchEmployeeDocName");
        }

        public IActionResult RemoveAdminDocName(int id)
        {
            var data = db.addAdminDocNames.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (data != null)
            {
                db.addAdminDocNames.Remove(data);
                db.SaveChanges();
                return RedirectToAction("FetchAdminDocName");
            }
            else
            {
                return null;
            }
        }

        public IActionResult RemoveEmployeeDocName(int id)
        {
            var data = db.addEmployeeDocNames.Where(x => x.Id.Equals(id)).FirstOrDefault();
            if (data != null)
            {
                db.addEmployeeDocNames.Remove(data);
                db.SaveChanges();
                return RedirectToAction("FetchEmployeeDocName");
            }
            else
            {
                return null;
            }
        }
    }
}
