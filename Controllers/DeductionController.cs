using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class DeductionController : Controller
    {
        private ApplicationDbContext db;
        public DeductionController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult DeductionType()
        {
            return View();
        }

        public IActionResult FetchDeductionType()
        {
            var data = db.DeductionType
                .Select(d => new
                {
                    d.DeductionTypeId,
                    d.DeductionsName,
                })
                .ToList();

            // Return the filtered data as JSON
            return Json(data);
        }

        [HttpPost]
        public IActionResult AddDeductionType(DeductionType deducttype)
        {
            db.DeductionType.Add(deducttype);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult FetchDeductionTypeDetails(int id)
        {
            var deductTypeDetail = db.DeductionType.Find(id);
            return Json(deductTypeDetail);
        }

        [HttpPost]
        public IActionResult UpdateDeductTypeDetails(DeductionType de)
        {
            db.DeductionType.Update(de);
            db.SaveChanges();
            return new JsonResult("");
        }

        public IActionResult DeleteDeductionType(int id)
        {
            var deductType = db.DeductionType.FirstOrDefault(x => x.DeductionTypeId == id);
            if (deductType != null)
            {
                db.DeductionType.Remove(deductType);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "DeductionType not found." });
        }

        ////Deduction
        public IActionResult Deduction()
        {
            ViewBag.deductionType = new SelectList(db.DeductionType, "DeductionTypeId", "DeductionsName");
            ViewBag.department = new SelectList(db.Departments, "DepartmentId", "Name");
            ViewBag.designation = new SelectList(db.Designations, "DesignationId", "Name");
            return View();
        }

        public IActionResult FetchDeduction()
        {
            var data = db.Deduction
                .Include(e => e.DeductionType)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Select(e => new
                {
                    e.DeductionId,
                    e.DeductionTypeId,
                    DeductionType = new { e.DeductionType.DeductionsName },
                    e.DeductionPercentage,
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

            //Return the filtered data as JSON
            return Json(data);
        }

        [HttpPost]
        public IActionResult AddDeduction(Deduction deduction)
        {
            var username = User.Identity.Name;

            deduction.CreatedBy = username;
            deduction.CreatedAt = DateTime.UtcNow;
            db.Deduction.Add(deduction);
            db.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult FindDeductionDetails(int id)
        {
            var DeductionDetail = db.Deduction
                .Include(e => e.DeductionType)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefault(e => e.DeductionId == id); // Use FirstOrDefault to avoid errors if not found

            if (DeductionDetail == null)
            {
                return Json(new { error = "Earning details not found" });
            }

            var deductionDto = new
            {
                deductionId = DeductionDetail.DeductionId,
                deductTypeId = DeductionDetail.DeductionType?.DeductionTypeId, // Avoid null reference
                deductionPercentage = DeductionDetail.DeductionPercentage,
                departmentId = DeductionDetail.Department?.DepartmentId,
                designationId = DeductionDetail.Designation?.DesignationId
            };

            return Json(deductionDto);
        }

        [HttpPost]
        public IActionResult UpdateDeductionDetails(Deduction deduct)
        {
            var username = User.Identity.Name;

            deduct.ModifiedBy = username;
            deduct.ModifiedAt = DateTime.UtcNow;
            db.Deduction.Update(deduct);
            db.SaveChanges();
            return new JsonResult("");
        }

        public IActionResult DeleteDeduction(int id)
        {
            var deduct = db.Deduction.FirstOrDefault(x => x.DeductionId == id);
            if (deduct != null)
            {
                db.Deduction.Remove(deduct);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Deductions not found." });
        }
    }
}
