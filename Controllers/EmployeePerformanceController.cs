using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;
using System.Security.Claims;

namespace Pulse360.Controllers
{
    public class EmployeePerformanceController : Controller
    {
        private readonly ApplicationDbContext db;

        public EmployeePerformanceController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // GET: EmployeePerformance/AddPReview

        public IActionResult FetchReview()
        {
            var data = db.EmployeePerformances

        .GroupBy(r => r.EmployeeId)  // Ensure there is only one record per EmployeeId
        .Select(g => g.FirstOrDefault()) // Take the first record from each group (which ensures uniqueness)
        .ToList();

            return View(data);


        }
        public IActionResult AddPReview()
        {

            ViewBag.EmployeeId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            return View();
        }

        // POST: EmployeePerformance/AddPReview
        // Receives a JSON array of EmployeePerformance records
        [HttpPost]
        public IActionResult AddPReview([FromBody] List<EmployeePerformance> performances)
        {
            if (performances == null || !performances.Any())
            {
                return Json(new { success = false, message = "No data provided" });
            }

            // Optionally enforce EmployeeId (or use values provided)
            foreach (var performance in performances)
            {
                // Ensure a valid EmployeeId is set
                performance.EmployeeId = performance.EmployeeId == 0 ? 1 : performance.EmployeeId;
                db.EmployeePerformances.Add(performance);
            }
            db.SaveChanges();

            return Json(new { success = true });
        }


        public IActionResult ReviewDetails(int id)
        {
            // Retrieve the review based on the id
            var data = db.EmployeePerformances
          .FirstOrDefault(r => r.ID == id); // Returns a single record

            return View(data);

        }
        public JsonResult GetEmployeeReviewData(int id)
        {
            var employee = db.EmployeePerformances.FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found." });
            }

            var personalRecords = db.EmployeePerformances
                .Where(e => e.EmployeeId == employee.EmployeeId && e.Category == "Personal Excellence")
                .ToList();

            var professionalRecords = db.EmployeePerformances
                .Where(e => e.EmployeeId == employee.EmployeeId && e.Category == "Professional Excellence")
                .ToList();

            return Json(new
            {
                success = true,
                EmployeeDetails = employee,
                PersonalExcellenceRecords = personalRecords,
                ProfessionalExcellenceRecords = professionalRecords
            });
        }
    }
}
