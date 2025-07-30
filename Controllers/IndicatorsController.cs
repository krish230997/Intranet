using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Pulse360.Controllers
{
    public class IndicatorsController : Controller
    {
        private readonly ApplicationDbContext db;
        public IndicatorsController(ApplicationDbContext db)
        {
            this.db = db;

        }
        public IActionResult Index()
        {
            var data = db.MasterIndicators.ToList();
            return View(data);



        }
        public IActionResult AddMIndicator()
        {
            return View();

        }
        [HttpPost]
        public IActionResult AddMIndicator(MasterIndicators mi)
        {


            db.MasterIndicators.Add(mi);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Fetch(string searchString)
        {
            ViewData["SearchString"] = searchString;

            var data = db.PerformanceIndicators.Include(x => x.Departments)
                                               .Include(x => x.Designations)
                                               .AsQueryable();


            string adminImageUrl = "/Content/uploads";


            var admin = db.User
    .Where(u => u.Role.RoleName == "Admin")
    .FirstOrDefault();


            ViewBag.AdminName = admin;


            if (!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(g => g.Departments.Name.Contains(searchString) ||
                                       g.Designations.Name.Contains(searchString) ||
                                       g.Status.Contains(searchString) ||
                                       g.ApprovedBy.Contains(searchString));
            }

            return View(data.ToList());
        }



        public IActionResult AddPIndicator()
        {
            ViewBag.MasterIndicatorType = new SelectList(db.MasterIndicators, "MasterIndicatorId", "MasterIndicatorType");
            ViewBag.MasterIndicatorName = new SelectList(db.MasterIndicators, "MasterIndicatorId", "MasterIndicatorName");
            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");
            ViewBag.Designations = new SelectList(db.Designations, "DesignationId", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult AddPIndicator(PerformanceIndicator p)
        {
            p.ApprovedBy = "Admin";


            p.CreatedAt = DateTime.Now;
            db.PerformanceIndicators.Add(p);
            db.SaveChanges();
            return RedirectToAction("Fetch");

        }

        public IActionResult EditIndicator(int id)
        {
            var indicator = db.PerformanceIndicators.Include(p => p.Designations)
                                          .Include(p => p.Departments)
                                          .FirstOrDefault(p => p.PerformanceIndicatorId == id);

            if (indicator == null)
            {
                return NotFound();
            }

            // Fetch the customer experience options (including the selected one)
            var allCustomerExperienceValues = new List<string> { "Advanced", "Intermediate", "Average" };

            // Ensure the selected value is part of the list, even if it’s already selected
            var customerExperienceValues = allCustomerExperienceValues.Where(x => x != indicator.CustomerExperience).ToList();
            customerExperienceValues.Insert(0, indicator.CustomerExperience);  // Insert selected value at the start

            // Pass the list with the selected value to the ViewBag
            ViewBag.CustomerExperience = new SelectList(customerExperienceValues);

            // Prepare the other dropdowns
            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name", indicator.DepartmentId);
            ViewBag.Designations = new SelectList(db.Designations, "DesignationId", "Name", indicator.DesignationId);

            return View(indicator);



            //         ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");
            //ViewBag.Designations = new SelectList(db.Designations, "DesignationId", "Name");
            //         ViewBag.CustomerExperience = new SelectList(db.PerformanceIndicators, "PerformanceIndicatorId", "CustomerExperience");

            //         var indicator = db.PerformanceIndicators.Find(id);
            //if (indicator == null)
            //{
            //	return NotFound();
            //}
            //return View(indicator);
        }

        [HttpPost]
        public IActionResult EditIndicator(PerformanceIndicator p)
        {
            // Validate the model state


            // Validate foreign key references
            if (!db.Departments.Any(d => d.DepartmentId == p.DepartmentId))
            {
                ModelState.AddModelError("DepartmentId", "The selected department does not exist.");
            }

            if (!db.Designations.Any(d => d.DesignationId == p.DesignationId))
            {
                ModelState.AddModelError("DesignationId", "The selected designation does not exist.");
            }




            // Find the existing record
            var existingIndicator = db.PerformanceIndicators
                          .FirstOrDefault(p => p.PerformanceIndicatorId == p.PerformanceIndicatorId);

            if (existingIndicator == null)
            {
                return NotFound();
            }


            // Update fields
            p.ApprovedBy = "Admin";

            p.CreatedAt = DateTime.Now;
            existingIndicator.DesignationId = p.DesignationId;
            existingIndicator.DepartmentId = p.DepartmentId;
            existingIndicator.CustomerExperience = p.CustomerExperience;
            existingIndicator.Marketing = p.Marketing;
            existingIndicator.Management = p.Management;
            existingIndicator.Administration = p.Administration;
            existingIndicator.PresentationSkills = p.PresentationSkills;
            existingIndicator.QualityofWork = p.QualityofWork;
            existingIndicator.Efficiency = p.Efficiency;


            existingIndicator.Integrity = p.Integrity;
            existingIndicator.Professionalism = p.Professionalism;
            existingIndicator.TeamWork = p.TeamWork;
            existingIndicator.CriticalThinking = p.CriticalThinking;
            existingIndicator.ConflictManagement = p.ConflictManagement;
            existingIndicator.Attendance = p.Attendance;
            existingIndicator.AbilityToMeetDeadline = p.AbilityToMeetDeadline;

            existingIndicator.Status = p.Status;

            db.SaveChanges();
            return RedirectToAction("Fetch");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var performanceIndicators = db.PerformanceIndicators.Find(id);



            if (performanceIndicators == null)
            {
                return NotFound();
            }

            {
                db.PerformanceIndicators.Remove(performanceIndicators);
                db.SaveChanges();
            }

            return RedirectToAction("Fetch");
        }

        public IActionResult FetchAppriasal(string searchString)
        {
            // Save the search string to ViewData for rendering in the search input field
            ViewData["SearchString"] = searchString;

            // Query the database and include related data
            var data = db.PerformanceAppriasal
                .Include(x => x.User)
                .Include(x => x.Departments)
                .Include(x => x.Designations)
                .AsQueryable();

            // Apply filtering if a search string is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(x => (x.User.FirstName + " " + x.User.LastName).Contains(searchString) ||
                                       (x.Designations.Name != null && x.Designations.Name.Contains(searchString)) ||
                                       (x.Departments.Name != null && x.Departments.Name.Contains(searchString)) ||
                                       (x.Status != null && x.Status.Contains(searchString)));
            }

            // Return the filtered or full data list to the view
            return View(data.ToList());
        }



        public IActionResult AddAppriasal()

        {


            var indicators = db.PerformanceIndicators.FirstOrDefault(); // Assuming one set of indicators for now

            // Pass the indicators to the view via ViewBag
            ViewBag.Indicators = indicators;

            ViewBag.ProfilePicture = new SelectList(db.User, "UserId", "ProfilePicture");
            //ViewBag.User = new SelectList(
            //	db.User.Select(u => new
            //	{
            //		UserId = u.UserId,
            //		FullName = u.FirstName + " " + u.LastName
            //	}),
            //	"UserId",
            //	"FullName"
            //);

            ViewBag.User = new SelectList(
    db.User
      .Where(u => u.Role.RoleName == "Employee" && u.Status.Equals("Active"))
      .Select(u => new
      {
          UserId = u.UserId,
          FullName = u.FirstName + " " + u.LastName
      }),
    "UserId",
    "FullName"
);


            ViewBag.Departments = new SelectList(db.Departments.Where(x => x.Status.Equals("Active")), "DepartmentId", "Name");
            ViewBag.Designations = new SelectList(db.Designations.Where(x=>x.status.Equals("Active")), "DesignationId", "Name");
            return View();
        }

        //    [HttpPost]
        //    public IActionResult AddAppriasal(PerformanceAppraisal pa)
        //    {
        //        if(ModelState.IsValid)
        //        {
        //            db.PerformanceAppriasal.Add(pa);
        //            db.SaveChanges();
        //            return RedirectToAction("FetchAppriasal");
        //        }
        //        else
        //        {
        //            var indicators = db.PerformanceIndicators.FirstOrDefault(); // Assuming one set of indicators for now

        //            // Pass the indicators to the view via ViewBag
        //            ViewBag.Indicators = indicators;

        //            ViewBag.ProfilePicture = new SelectList(db.User, "UserId", "ProfilePicture");
        //            //ViewBag.User = new SelectList(
        //            //	db.User.Select(u => new
        //            //	{
        //            //		UserId = u.UserId,
        //            //		FullName = u.FirstName + " " + u.LastName
        //            //	}),
        //            //	"UserId",
        //            //	"FullName"
        //            //);

        //            ViewBag.User = new SelectList(
        //    db.User
        //      .Where(u => u.Role.RoleName == "Employee")
        //      .Select(u => new
        //      {
        //          UserId = u.UserId,
        //          FullName = u.FirstName + " " + u.LastName
        //      }),
        //    "UserId",
        //    "FullName"
        //);


        //            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");
        //            ViewBag.Designations = new SelectList(db.Designations, "DesignationId", "Name");
        //            TempData["msg"] = "Fields Should be not Empty!!";
        //            return View();
        //        }

        //    }

        [HttpPost]
        public IActionResult AddAppriasal(PerformanceAppraisal pa)
        {
          
                try
                {
                   
                    db.PerformanceAppriasal.Add(pa);
                    db.SaveChanges();
                    TempData["success"] = "Appraisal added successfully!";
                    return RedirectToAction("FetchAppriasal");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error saving appraisal: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving the appraisal. Please try again.");
                }
            return RedirectToAction("FetchAppriasal");

        }

        private void LoadDropdowns()
        {
            ViewBag.User = new SelectList(
                db.User.Where(u => u.Role.RoleName == "Employee")
                .Select(u => new { UserId = u.UserId, FullName = u.FirstName + " " + u.LastName }),
                "UserId", "FullName"
            );

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");
            ViewBag.Designations = new SelectList(db.Designations, "DesignationId", "Name");
        }



        public IActionResult EditAppriasal(int id)
        {
            var appraisal = db.PerformanceAppriasal
                .Include(pa => pa.User)
                .Include(pa => pa.Departments)
                .Include(pa => pa.Designations)
                .FirstOrDefault(pa => pa.PerformanceAppriasalId == id);

            if (appraisal == null)
            {
                return NotFound("Appraisal not found.");
            }

            var indicators = db.PerformanceIndicators.FirstOrDefault();

            ViewBag.Indicators = indicators;

            // Populate dropdowns
            ViewBag.User = new SelectList(
                db.User.Select(u => new
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                }),
                "UserId",
                "FullName",
                appraisal.UserId
            );

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name", appraisal.DepartmentId);
            ViewBag.Designations = new SelectList(db.Designations, "DesignationId", "Name", appraisal.DesignationId);

            return View(appraisal);
        }

        [HttpPost]
        public IActionResult EditAppriasal(PerformanceAppraisal pa)
        {

            // If validation fails, repopulate dropdowns
            ViewBag.User = new SelectList(
                db.User.Select(u => new
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                }),
                "UserId",
                "FullName",
                pa.UserId
            );

            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name", pa.DepartmentId);
            ViewBag.Designations = new SelectList(db.Designations, "DesignationId", "Name", pa.DesignationId);



            var existingAppraisal = db.PerformanceAppriasal.Find(pa.PerformanceAppriasalId);


            // Update the properties with the new values from the form
            existingAppraisal.UserId = pa.UserId;
            existingAppraisal.DesignationId = pa.DesignationId;
            existingAppraisal.DepartmentId = pa.DepartmentId;
            existingAppraisal.Status = pa.Status;
            existingAppraisal.AppraisalDate = pa.AppraisalDate;

            // Update competencies
            existingAppraisal.CustomerExperience = pa.CustomerExperience;
            existingAppraisal.Marketing = pa.Marketing;
            existingAppraisal.Management = pa.Management;
            existingAppraisal.Administration = pa.Administration;
            existingAppraisal.PresentationSkills = pa.PresentationSkills;
            existingAppraisal.QualityofWork = pa.QualityofWork;
            existingAppraisal.Efficiency = pa.Efficiency;
            existingAppraisal.Integrity = pa.Integrity;
            existingAppraisal.Professionalism = pa.Professionalism;
            existingAppraisal.TeamWork = pa.TeamWork;
            existingAppraisal.CriticalThinking = pa.CriticalThinking;
            existingAppraisal.ConflictManagement = pa.ConflictManagement;
            existingAppraisal.Attendance = pa.Attendance;
            existingAppraisal.AbilityToMeetDeadline = pa.AbilityToMeetDeadline;

            db.SaveChanges();

            return RedirectToAction("FetchAppriasal");
        }

        [HttpPost]
        public IActionResult DeleteApprisal(int id)
        {
            var performanceApprisal = db.PerformanceAppriasal.Find(id);



            if (performanceApprisal == null)
            {
                return NotFound();
            }

            {
                db.PerformanceAppriasal.Remove(performanceApprisal);
                db.SaveChanges();
            }

            return RedirectToAction("FetchAppriasal");
        }



        //[HttpPost]
        //public IActionResult PerformanceAppraisal(string action, PerformanceAppriasal model)
        //{
        //    if (action == "Technical")
        //    {
        //        // Handle logic for Technical fields
        //        var technicalData = new
        //        {
        //            model.CustomerExperience,
        //            model.Marketing,
        //            model.Management,
        //            model.Administration,
        //            model.PresentationSkills,
        //            model.QualityofWork,
        //            model.Efficiency
        //        };

        //        // Process technical data
        //        // For example: Save or process the data as needed
        //    }
        //    else if (action == "Organizational")
        //    {
        //        // Handle logic for Organizational fields
        //        var organizationalData = new
        //        {
        //            model.Integrity,
        //            model.Professionalism,
        //            model.TeamWork,
        //            model.CriticalThinking,
        //            model.ConflictManagement,
        //            model.Attendance,
        //            model.AbilityToMeetDeadline
        //        };

        //        // Process organizational data
        //        // For example: Save or process the data as needed
        //    }

        //    // You can redirect or return a view as necessary
        //    return View();
        //}


        public IActionResult FetchReview(string sortBy = "recent")
        {
            var query = db.PerformanceReviews.AsQueryable();

            switch (sortBy.ToLower())
            {
                case "asc":
                    query = query.OrderBy(x => x.DateofJoin);
                    break;
                case "desc":
                case "recent":
                    query = query.OrderByDescending(x => x.DateofJoin);
                    break;
                case "last7days":
                    query = query.Where(x => x.DateofJoin >= DateTime.Now.AddDays(-7))
                                 .OrderByDescending(x => x.DateofJoin);
                    break;
                case "lastmonth":
                    query = query.Where(x => x.DateofJoin >= DateTime.Now.AddMonths(-1))
                                 .OrderByDescending(x => x.DateofJoin);
                    break;
                default:
                    query = query.OrderByDescending(x => x.DateofJoin);
                    break;
            }

            return View(query.ToList());


        }
        public IActionResult AddPReview()
        {
            return View();

        }
        [HttpPost]
        public IActionResult AddPReview(PerformanceReview mi)
        {


            db.PerformanceReviews.Add(mi);
            db.SaveChanges();
            return RedirectToAction("FetchReview");
        }


        public IActionResult FilteredAttendanceAction(string searchString, string status, DateTime? startDate, DateTime? endDate, int rowsPerPage = 10, int page = 1, string sortBy = "recent")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = db.User.SingleOrDefault(x => x.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            // Ensure related entities are included
            var query = db.PerformanceIndicators
                .Include(x => x.Designations)
                .Include(x => x.Departments)
                .Where(x => x.PerformanceIndicatorId == user.UserId)
                .AsQueryable();

            // ✅ Search Filtering
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(x =>
                    (x.Designations != null && x.Designations.Name != null && x.Designations.Name.ToLower().Contains(searchString)) ||
                    (x.Departments != null && x.Departments.Name != null && x.Departments.Name.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(x.ApprovedBy) && x.ApprovedBy.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(x.Status) && x.Status.ToLower().Contains(searchString))
                );
            }

            // ✅ Status Filtering
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(x => x.Status != null && x.Status.ToLower() == status.ToLower());
            }

            // ✅ Date Filtering (Start & End Date)
            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= endDate.Value);
            }

            // ✅ Sorting Logic
            switch (sortBy.ToLower())
            {
                case "asc":
                    query = query.OrderBy(x => x.CreatedAt);
                    break;
                case "desc":
                case "recent":
                    query = query.OrderByDescending(x => x.CreatedAt);
                    break;
                case "last7days":
                    query = query.Where(x => x.CreatedAt >= DateTime.Now.AddDays(-7)).OrderByDescending(x => x.CreatedAt);
                    break;
                case "lastmonth":
                    query = query.Where(x => x.CreatedAt >= DateTime.Now.AddMonths(-1)).OrderByDescending(x => x.CreatedAt);
                    break;
                default:
                    query = query.OrderByDescending(x => x.CreatedAt);
                    break;
            }

            // ✅ Pagination Logic
            int totalRecords = query.Count();
            var attendanceList = query.Skip((page - 1) * rowsPerPage).Take(rowsPerPage).ToList();

            // ✅ Debugging: Print SQL Query (Check if filtering is working)
            Console.WriteLine(query.ToQueryString()); // Prints the generated SQL query

            // ✅ Pass Data to View
            ViewData["SearchString"] = searchString;
            ViewData["Status"] = status;
            ViewData["RowsPerPage"] = rowsPerPage;
            ViewData["TotalRecords"] = totalRecords;
            ViewData["CurrentPage"] = page;
            ViewData["SortBy"] = sortBy;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;

            return Json(new { data = attendanceList, totalRecords });
        }

    }
}
