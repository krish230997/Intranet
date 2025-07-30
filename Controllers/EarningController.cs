using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class EarningController : Controller
    {
        private ApplicationDbContext db;
        public EarningController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult EarningType()
        {
            return View();
        }

        public IActionResult FetchEarningType()
        {
            var data = db.EarningType
                .Select(et => new
                {
                    et.EarntypeId,
                    et.EarningName,
                })
                .ToList();

            // Return the filtered data as JSON
            return Json(data);
        }

        [HttpPost]
        public IActionResult AddEarningType(EarningType earntype)
        {
            db.EarningType.Add(earntype);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult FindEarningTypeDetails(int id)
        {
            var eranTypeDetail = db.EarningType.Find(id);
            return Json(eranTypeDetail);
        }

        [HttpPost]
        public IActionResult UpdateEarnTypeDetails(EarningType et)
        {
            db.EarningType.Update(et);
            db.SaveChanges();
            return new JsonResult("");
        }

        public IActionResult DeleteEarningType(int id)
        {
            var earnType = db.EarningType.FirstOrDefault(x => x.EarntypeId == id);
            if (earnType != null)
            {
                db.EarningType.Remove(earnType);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "EarningType not found." });
        }

        //Earnings
        public IActionResult Earning()
        {
            ViewBag.earningType = new SelectList(db.EarningType, "EarntypeId", "EarningName");
            ViewBag.department = new SelectList(db.Departments, "DepartmentId", "Name");
            ViewBag.designation = new SelectList(db.Designations, "DesignationId", "Name");
            return View();
        }

        public IActionResult FetchEarning()
        {
            var data = db.Earning
                .Include(e => e.EarningType)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Select(e => new
                {
                    e.EarningsId,
                    e.EarntypeId,
                    EarningType = new { e.EarningType.EarningName },
                    e.EarningsPercentage,
                    e.DepartmentId,
                    Department = new { e.Department.Name },
                    e.DesignationId,
                    Designation = new { e.Designation.Name },
                    e.CreatedBy,
                    e.CreatedAt,
                    e.ModifiedBy,
                    e.ModifiedAt
                })
                .ToList();

            // Return the filtered data as JSON
            return Json(data);
        }

        [HttpPost]
        public IActionResult AddEarning(Earning earning)
        {
            var username = User.Identity.Name;

            earning.CreatedBy = username;
            earning.CreatedAt = DateTime.UtcNow;
            db.Earning.Add(earning);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult FindEarningDetails(int id)
        {
            var earningDetail = db.Earning
                .Include(e => e.EarningType)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefault(e => e.EarningsId == id); // Use FirstOrDefault to avoid errors if not found

            if (earningDetail == null)
            {
                return Json(new { error = "Earning details not found" });
            }

            var earningDto = new
            {
                earningsId = earningDetail.EarningsId,
                earntypeId = earningDetail.EarningType?.EarntypeId, // Avoid null reference
                earningsPercentage = earningDetail.EarningsPercentage,
                departmentId = earningDetail.Department?.DepartmentId,
                designationId = earningDetail.Designation?.DesignationId
            };

            return Json(earningDto);
        }

        [HttpPost]
        public IActionResult UpdateEarningDetails(Earning earn)
        {
            var username = User.Identity.Name;

            earn.ModifiedBy = username;
            earn.ModifiedAt = DateTime.UtcNow;
            db.Earning.Update(earn);
            db.SaveChanges();
            return new JsonResult("");
        }

        public IActionResult DeleteEarning(int id)
        {
            var earn = db.Earning.FirstOrDefault(x => x.EarningsId == id);
            if (earn != null)
            {
                db.Earning.Remove(earn);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Earning not found." });
        }
    }
}
