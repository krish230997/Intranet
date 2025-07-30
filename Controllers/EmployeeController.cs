using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
//using PdfSharp.Drawing;
using Pulse360.Data;
using Pulse360.Models;
using static System.Net.Mime.MediaTypeNames;
using Department = Pulse360.Models.Department;

namespace Pulse360.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext db;
        public EmployeeController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Department()
        {
            return View();
        }
        public IActionResult FetchDept()
        {
            //var data = db.Departments.ToList();
            var data = db.Departments
                 .Select(d => new
                 {
                     d.DepartmentId,   // Department ID
                     d.Name,           // Department Name
                     NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
                     d.CreatedBy,
                     d.ModifiedBy,
                     d.Status          // Department Status
                 })
                 .ToList();

            
            ViewBag.Dept = new SelectList(
                db.Designations.ToList(), "DepartmentId", "Status"
            );

            return Json(data);
        }
        //public IActionResult AddDepartment(Department d)
        //{
        //	db.Departments.Add(d);
        //	db.SaveChanges();
        //	return Json("");
        //}
        [HttpPost]
        public IActionResult AddNewDepartment(Department d)
        {
            string email = HttpContext.Session.GetString("Email");
            var user = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

            if (user != null)
            {
                d.CreatedBy = user.FirstName;  // Assign FirstName to CreatedBy
                d.CreatedAt = DateTime.UtcNow; // Set CreatedAt timestamp
            }
            db.Departments.Add(d);
            db.SaveChanges();
            return Json("");
        }


        [HttpGet] // Ensure GET method is specified
        public IActionResult EditDept(int eid)
        {
            Console.WriteLine($"EditRole action hit with eid: {eid}"); // Debugging log

            var depts = db.Departments.Find(eid);
            if (depts == null)
            {
                return Json(new { success = false, message = "Dept not found." });
            }

            // Default status options
            List<string> statuses = new List<string> { "Active", "Inactive" };

            return Json(new { success = true, data = depts, statuses });
        }




        public IActionResult EditDepartment(Department d)
        {
            var dept = db.Departments.FirstOrDefault(x => x.DepartmentId == d.DepartmentId);
            if (dept != null)
            {
                string email = HttpContext.Session.GetString("Email");
                var user = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

                if (user != null)
                {
                    dept.ModifiedBy = user.FirstName;  // Assign FirstName to ModifiedBy
                    dept.ModifiedAt = DateTime.UtcNow; // ✅ Store as DateTime instead of string
                }
                dept.Name = d.Name;
                dept.Status = d.Status;
                db.SaveChanges();
                return Json(new { success = true });
            }

            
            return Json(new { success = false, message = "Department not found." });
        }

        public IActionResult DeleteDepartment(int id)
        {
            var dept = db.Departments.Find(id);
            if (dept != null)
            {
                db.Departments.Remove(dept);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Department not found." });
        }
        public IActionResult SearchDept(string mydata)
        {
            if (mydata != null)
            {
                var data = db.Departments.Where(x => x.Name.Contains(mydata))
                    .Select(d => new
                {
                    d.DepartmentId,   // Department ID
                    d.Name,           // Department Name
                    NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
                    d.Status          // Department Status
                }).ToList();
                return Json(data);
            }
            else
            {
                //var data = db.Departments.ToList();
                //return Json(data);

                var data = db.Departments
                 .Select(d => new
                 {
                     d.DepartmentId,   // Department ID
                     d.Name,           // Department Name
                     NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
                     d.Status          // Department Status
                 })
                 .ToList();
                return Json(data);
            }
        }

        public IActionResult Sorting(string mydata)
        {
            if (mydata.Equals("asc"))
            {
                var data = db.Departments.OrderBy(x => x.DepartmentId)
                    .Select(d => new
                    {
                        d.DepartmentId,   // Department ID
                        d.Name,           // Department Name
                        NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
                        d.Status,          // Department Status
                        d.CreatedBy,
                        d.ModifiedBy
                    }).ToList();
                return Json(data);
            }
            else if (mydata.Equals("desc"))
            {
                var data = db.Departments.OrderByDescending(x => x.DepartmentId)
                    .Select(d => new
                    {
                        d.DepartmentId,   // Department ID
                        d.Name,           // Department Name
                        NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
                        d.Status,          // Department Status
						d.CreatedBy,
						d.ModifiedBy
					}).ToList();
                return Json(data);
            }
            else
            {
                var data = db.Departments
                    .Select(d => new
                    {
                        d.DepartmentId,   // Department ID
                        d.Name,           // Department Name
                        NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
                        d.Status,          // Department Status
                        d.CreatedBy,
                        d.ModifiedBy
                    }).ToList();
                return Json(data);
            }
        }
		//public IActionResult SortingStatus(string mydata)
		//{
		//    if (!string.IsNullOrEmpty(mydata))
		//    {
		//        var data = db.Departments.Where(x => x.Status == mydata)
		//            .Select(d => new
		//            {
		//                d.DepartmentId,   // Department ID
		//                d.Name,           // Department Name
		//                NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
		//                d.Status,          // Department Status
		//                d.CreatedBy,
		//                d.ModifiedBy
		//            }).ToList();
		//        return Json(data);
		//    }
		//    else
		//    {
		//        var data = db.Departments
		//            .Select(d => new
		//            {
		//                d.DepartmentId,   // Department ID
		//                d.Name,           // Department Name
		//                NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
		//                d.CreatedBy,
		//                d.ModifiedBy,
		//                d.Status          // Department Status
		//            }).ToList();
		//        return Json(data);
		//    }
		//}
		public IActionResult SortingStatus(string mydata)
		{
			if (mydata == "select")
			{
				// Return all departments when "select" is chosen
				var data = db.Departments
					.Select(d => new
					{
						d.DepartmentId,   // Department ID
						d.Name,           // Department Name
						NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
						d.Status,         // Department Status
						d.CreatedBy,
						d.ModifiedBy
					})
					.ToList();
				return Json(data);
			}
			else if (!string.IsNullOrEmpty(mydata))
			{
				// Filter departments based on the selected status
				var data = db.Departments
					.Where(x => x.Status == mydata)
					.Select(d => new
					{
						d.DepartmentId,   // Department ID
						d.Name,           // Department Name
						NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
						d.Status,         // Department Status
						d.CreatedBy,
						d.ModifiedBy
					})
					.ToList();
				return Json(data);
			}
			else
			{
				// Default case: return all departments
				var data = db.Departments
					.Select(d => new
					{
						d.DepartmentId,   // Department ID
						d.Name,           // Department Name
						NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
						d.Status          // Department Status
					})
					.ToList();
				return Json(data);
			}
		}
		public IActionResult GetDepartments(int rows)
        {
            var departments = db.Departments.Take(rows)
                .Select(d => new
                {
                    d.DepartmentId,   // Department ID
                    d.Name,           // Department Name
                    NoOfEmployee = db.User.Count(e => e.DepartmentId == d.DepartmentId), // Count employees in department
                    d.Status          // Department Status
                })
                .ToList();
            return Json(departments);
        }




        //start  with designation part
        //public IActionResult Designation()
        //{
        //	ViewBag.Departments = db.Departments.Select(d => new SelectListItem
        //	{
        //		Value = d.DepartmentId.ToString(),
        //		Text = d.Name
        //	}).ToList();

        //	ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentId", "Name");

        //	return View();


        //}
        public IActionResult Designation()
        {
            ViewBag.Departments = db.Departments
                .Where(d => d.Status == "Active") // Filter only active departments
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name
                })
                .ToList();

            return View();
        }


        public IActionResult SortingUserByDepartment(int departmentId)
        {
            if (departmentId != 0)
            {
                var data = db.User.Where(x => x.DepartmentId == departmentId)
                                  .Include(x => x.Designation)
                                  .Include(x => x.Department)
                                  .Include(x => x.Role)
                                  .ToList();
                return Json(data);
            }
            else
            {
                var data = db.User.Include(x => x.Designation)
                                  .Include(x => x.Department)
                                  .Include(x => x.Role)
                                  .ToList();
                return Json(data);
            }
        }

        public IActionResult FetchDesignation()
        {

            //var data = db.Designation.Include(x => x.Department).ToList();
            //var data = db.Designation.ToList();
            //return Json(data);

            var data = db.Designations
                .Include(u => u.Department)
                .Select(u => new
                {
                    u.DesignationId,
                    u.Name,
                    u.CreatedBy,
                    u.ModifiedBy,
                    //u.DepartmentId,
                    //Role = new { RoleName = u.Role.RoleName },
                    Department = new { u.Department.Name },
                    NoOfEmployee = db.User.Count(e => e.DesignationtId == u.DesignationId),
                    u.status
                })
                .ToList();



            // Return the filtered data as JSON
            return Json(data);

        }


        public IActionResult AddDesignation(Designation d)
        {
            string email = HttpContext.Session.GetString("Email");
            var user = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

            if (user != null)
            {
                d.CreatedBy = user.FirstName;  // Assign FirstName to CreatedBy
                d.CreatedAt = DateTime.UtcNow; // Set CreatedAt timestamp
            }
            db.Designations.Add(d);
            db.SaveChanges();
            return Json("");
        }

        //public IActionResult EditDesignationDetails(Designation d)
        //{
        //    var desi = db.Designations.FirstOrDefault(x => x.DesignationId == d.DesignationId);
        //    if (desi != null)
        //    {
        //        string email = HttpContext.Session.GetString("Email");
        //        var user = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

        //        if (user != null)
        //        {
        //            desi.ModifiedBy = user.FirstName;  // Assign FirstName to ModifiedBy
        //            desi.ModifiedAt = DateTime.UtcNow; // ✅ Store as DateTime instead of string
        //        }
        //        desi.Name = d.Name;
        //        desi.Department = d.Department;
        //        desi.status = d.status;
        //        db.Designations.Update(desi);
        //        db.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false, message = "Department not found." });
        //}

        public IActionResult EditDesignation(int eid)
        {
            var designation = db.Designations.Find(eid);
            if (designation == null)
            {
                return Json(new { success = false, message = "Designation not found." });
            }

            // Get active departments
            var departments = db.Departments
                                .Where(x => x.Status.Equals("Active"))
                                .Select(d => new { departmentId = d.DepartmentId, name = d.Name })
                                .ToList();

            // Default status options
            List<string> statuses = new List<string> { "Active", "Inactive" };

            return Json(new { success = true, data = designation, departments, statuses });
        }




        [HttpPost]
        public IActionResult EditDesignationDetails([FromBody]Designation d)
        {
            if (d == null || d.DesignationId == 0)
            {
                return Json(new { success = false, message = "Invalid designation data." });
            }

            var desi = db.Designations.FirstOrDefault(x => x.DesignationId == d.DesignationId);
            if (desi == null)
            {
                return Json(new { success = false, message = "Designation not found." });
            }

            string email = HttpContext.Session.GetString("Email");
            var user = db.User.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                desi.ModifiedBy = user.FirstName;  // Assign FirstName to ModifiedBy
                desi.ModifiedAt = DateTime.UtcNow; // ✅ Store as DateTime instead of string
            }

            desi.Name = d.Name;
            desi.DepartmentId = d.DepartmentId;  // ✅ Corrected: Use DepartmentId instead of Department
            desi.status = d.status;              // ✅ Corrected: Ensure property name matches model

            db.Designations.Update(desi);
            db.SaveChanges();

            return Json(new { success = true });
        }


        public IActionResult DeleteDesignation(int id)
        {
            var desi = db.Designations.FirstOrDefault(x => x.DesignationId == id);
            if (desi != null)
            {
                db.Designations.Remove(desi);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Department not found." });
        }


        public IActionResult DesiSorting(string mydata)
        {
            if (mydata.Equals("asc"))
            {
                var data = db.Designations.OrderBy(x => x.DesignationId).Include(u => u.Department)
                .Select(u => new
                {
                    u.DesignationId,
                    u.Name,
                    //u.DepartmentId,
                    //Role = new { RoleName = u.Role.RoleName },
                    Department = new { Name = u.Department.Name },
                    NoOfEmployee = db.User.Count(e => e.DesignationtId == u.DesignationId),
                    u.status,
                    u.CreatedBy,
                    u.ModifiedBy
                }).ToList();
                return Json(data);
            }
            else if (mydata.Equals("desc"))
            {
                var data = db.Designations.OrderByDescending(x => x.DesignationId).Include(u => u.Department)
                .Select(u => new
                {
                    u.DesignationId,
                    u.Name,
                    //u.DepartmentId,
                    //Role = new { RoleName = u.Role.RoleName },
                    Department = new { Name = u.Department.Name },
                    NoOfEmployee = db.User.Count(e => e.DesignationtId == u.DesignationId),
                    u.status,
                    u.CreatedBy,
                    u.ModifiedBy
                }).ToList();
                return Json(data);
            }
            else
            {
                var data = db.Designations.ToList();
                return Json(data);
            }
        }
        public IActionResult DesiSortingStatus(string mydata)
        {
            //if (!string.IsNullOrEmpty(mydata))
            if(mydata.Equals("Active"))
            {
                var data = db.Designations.Where(x => x.status == mydata).Include(u => u.Department)
                .Select(u => new
                {
                    u.DesignationId,
                    u.Name,
                    //u.DepartmentId,
                    //Role = new { RoleName = u.Role.RoleName },
                    Department = new { Name = u.Department.Name },
                    NoOfEmployee = db.User.Count(e => e.DesignationtId == u.DesignationId),
                    u.status,
                    u.CreatedBy,
                    u.ModifiedBy
                }).ToList();
                return Json(data);
            }
			else if (mydata.Equals("Inactive"))
			{
				var data = db.Designations.Where(x => x.status == mydata).Include(u => u.Department)
				.Select(u => new
				{
					u.DesignationId,
					u.Name,
					//u.DepartmentId,
					//Role = new { RoleName = u.Role.RoleName },
					Department = new { Name = u.Department.Name },
					NoOfEmployee = db.User.Count(e => e.DesignationtId == u.DesignationId),
					u.status,
					u.CreatedBy,
					u.ModifiedBy
				}).ToList();
				return Json(data);
			}
			else
            {
                var data = db.Designations.ToList();
                return Json(data);
            }
        }


        public IActionResult GetEntries(int count)
        {
            var data = db.Designations.Take(count).Include(u => u.Department)
                .Select(u => new
                {
                    u.DesignationId,
                    u.Name,
                    //u.DepartmentId,
                    //Role = new { RoleName = u.Role.RoleName },
                    Department = new { Name = u.Department.Name },
                    NoOfEmployee = db.User.Count(e => e.DesignationtId == u.DesignationId),
                    u.status
                }).ToList(); // Get only the specified number of entries
            return Json(data);
        }



        //start with role section

        public IActionResult Role()
        {
            var rl = db.Role.Select(r => new SelectListItem
            {
                Text = r.RoleName,  // Assuming DepartmentName is the column
                Value = r.RoleName   // Assuming Id is the primary key
            }).ToList();

            ViewBag.Roles = rl;
            return View();
        }
        public IActionResult FetchRole()
        {
            var data = db.Role.ToList();

            return Json(data);
        }

        [HttpGet] // Ensure GET method is specified
        public IActionResult EditRole(int eid)
        {
            Console.WriteLine($"EditRole action hit with eid: {eid}"); // Debugging log

            var role = db.Role.Find(eid);
            if (role == null)
            {
                return Json(new { success = false, message = "Role not found." });
            }

            // Default status options
            List<string> statuses = new List<string> { "Active", "Inactive" };

            return Json(new { success = true, data = role, statuses });
        }

        public IActionResult AddRole(Role r)
        {
            string email = HttpContext.Session.GetString("Email");
            var user = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

            if (user != null)
            {
                r.CreatedBy = user.FirstName;  // Assign FirstName to CreatedBy
                r.CreatedAt = DateTime.UtcNow; // Set CreatedAt timestamp
            }
            db.Role.Add(r);
            db.SaveChanges();
            return Json("");
        }

        public IActionResult EditRole(Role r)
        {
            var rl = db.Role.FirstOrDefault(x => x.RoleId == r.RoleId);

            if (rl != null)
            {
                string email = HttpContext.Session.GetString("Email");
                var user = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

                if (user != null)
                {
                    rl.ModifiedBy = user.FirstName;  // Assign FirstName to ModifiedBy
                    rl.ModifiedAt = DateTime.UtcNow; // ✅ Store as DateTime instead of string
                }
                rl.RoleName = r.RoleName;
                rl.Status = r.Status;
                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Department not found." });
            }

        }

        public IActionResult DeleteRole(int id)
        {
            var rl = db.Role.FirstOrDefault(x => x.RoleId == id);
            if (rl != null)
            {
                db.Role.Remove(rl);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Department not found." });
        }
        public IActionResult SearchRole(string mydata)
        {
            if (mydata != null)
            {
                var data = db.Role.Where(x => x.RoleName.Contains(mydata)).ToList();
                return Json(data);
            }
            else
            {
                var data = db.Role.ToList();
                return Json(data);
            }
        }

        public IActionResult RoleSorting(string mydata)
        {
            if (mydata.Equals("asc"))
            {
                var data = db.Role.OrderBy(x => x.RoleId).ToList();
                return Json(data);
            }
            else if (mydata.Equals("desc"))
            {
                var data = db.Role.OrderByDescending(x => x.RoleId).ToList();
                return Json(data);
            }
            else if (mydata.Equals("select"))
            {
                var data = db.Role.ToList();
                return Json(data);
            }
            else 
            {
				var data = db.Role.ToList();
				return Json(data);
			}
        }
		//     public IActionResult RoleSortingStatus(string mydata)
		//     {
		//         if (!string.IsNullOrEmpty(mydata))
		//         {
		//             var data = db.Role.Where(x => x.Status == mydata).ToList();
		//             return Json(data);
		//         }
		//else if (mydata.Equals("select"))
		//{
		//	var data = db.Role.ToList();
		//	return Json(data);
		//}
		//else 
		//         {
		//             var data = db.Role.ToList();
		//             return Json(data);
		//         }
		//     }
		public IActionResult RoleSortingStatus(string mydata)
		{
			if (mydata == "select")
			{
				// Return all roles when "select" is chosen
				var data = db.Role.ToList();
				return Json(data);
			}
			else if (!string.IsNullOrEmpty(mydata))
			{
				// Filter roles based on the selected status
				var data = db.Role.Where(x => x.Status == mydata).ToList();
				return Json(data);
			}
			else
			{
				// Default case: return all roles
				var data = db.Role.ToList();
				return Json(data);
			}
		}
		public IActionResult GetRoleEntries(int count)
        {
            var data = db.Role.Take(count).ToList(); // Get only the specified number of entries
            return Json(data);
        }

        //start with User
        public IActionResult EmpList()
        {
            var managerRoleId = db.Role
        .Where(r => r.RoleName == "Manager")
        .Select(r => r.RoleId)
        .FirstOrDefault();

            // Fetch users where RoleId matches the Manager role
            var managers = db.User
                .Where(e => e.RoleId == managerRoleId)
                .Select(e => new
                {
                    Value = e.FirstName.ToString(),
                    Text = e.FirstName + " " + e.LastName,
                    DepartmentId = e.DepartmentId // Include DepartmentId for filtering
                })
                .ToList();

            ViewBag.Managers = managers;

            //for filter 
            //ViewBag.Designation = db.Designations.Select(d => new SelectListItem
            //{
            //	Value = d.DesignationId.ToString(),
            //	Text = d.Name
            //}).ToList();
            ////

            //ViewBag.Roles = new SelectList(db.Role.ToList(), "RoleId", "RoleName");
            //ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentId", "Name");
            //ViewBag.Designations = new SelectList(db.Designations.ToList(), "DesignationId", "Name");
            // Fetch only active designations
            ViewBag.Designation = db.Designations
                .Where(d => d.status == "Active")
                .Select(d => new SelectListItem
                {
                    Value = d.DesignationId.ToString(),
                    Text = d.Name
                })
                .ToList();

            // Fetch active roles
            //ViewBag.Roles = new SelectList(
            //    db.Role.Where(r => r.Status == "Active").ToList(), "RoleId", "RoleName"
            //);
            // For Roles
            ViewBag.Roles = new SelectList(
                db.Role.Where(r => r.Status == "Active").ToList(), "RoleId", "RoleName"
            );

            // Fetch active departments
            ViewBag.Departments = new SelectList(
                db.Departments.Where(d => d.Status == "Active").ToList(), "DepartmentId", "Name"
            );

            // Fetch active designations for dropdown
            ViewBag.Designations = new SelectList(
                db.Designations.Where(d => d.status == "Active").ToList(), "DesignationId", "Name"
            );


            ViewBag.ActiveMemberCount = db.User.Count(m => m.Status == "Active");
            ViewBag.InactiveMemberCount = db.User.Count(m => m.Status == "Inactive");
            ViewBag.TotalMemberCount = db.User.Count();
            ViewBag.RecentlyAddedCount = db.User.Count(m => m.DateOfJoining >= DateTime.Now.AddDays(-7));


            //ViewBag.Manager = manager;
            //return View(data);
            return View();
        }


        //Designation Filter in EmpList
        public IActionResult SortingUserByDesignation(string designationName)
        {
            try
            {
                if (!string.IsNullOrEmpty(designationName) && designationName != "Select Designation")
                {
                    var data = db.User
                        .Include(x => x.Designation)
                        .Include(x => x.Department)
                        .Include(x => x.Role).Where(x=>x.Role.RoleName!="Admin")
                        .Where(x => x.Designation.Name == designationName)
                        .Select(x => new
                        {
                            UserId = x.UserId,
                            ProfilePicture = x.ProfilePicture,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            Email = x.Email,
                            PhoneNumber = x.PhoneNumber,
                            RoleName = x.Role != null ? x.Role.RoleName : "N/A",
                            DepartmentName = x.Department != null ? x.Department.Name : "N/A",
                            DesignationName = x.Designation != null ? x.Designation.Name : "N/A",
                            ReportingManager = x.ReportingManager,
                            DateOfJoining = x.DateOfJoining.ToString("yyyy-MM-dd"), // Format date correctly
                            Status = x.Status,
                            x.CreatedBy,
                            x.ModifiedBy
                        })
                        .ToList();

                    if (data.Count == 0)
                    {
                        return Json(new { error = "No users found for the given designation." });
                    }

                    return Json(data);
                }

                return Json(new { error = "Invalid designation name." });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Server Error", details = ex.Message });
            }
        }


        public IActionResult FilterUsersByDate(string startDate, string endDate)
        {
            try
            {
                var query = db.User
                    .Include(x => x.Designation)
                    .Include(x => x.Department)
                    .Include(x => x.Role).Where(x => x.Role.RoleName != "Admin")
                    .AsQueryable();

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime start))
                {
                    query = query.Where(x => x.DateOfJoining >= start);
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime end))
                {
                    query = query.Where(x => x.DateOfJoining <= end);
                }

                var data = query.Select(x => new
                {
                    UserId = x.UserId,
                    ProfilePicture = x.ProfilePicture,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RoleName = x.Role != null ? x.Role.RoleName : "N/A",
                    DepartmentName = x.Department != null ? x.Department.Name : "N/A",
                    DesignationName = x.Designation != null ? x.Designation.Name : "N/A",
                    ReportingManager = x.ReportingManager,
                    DateOfJoining = x.DateOfJoining.ToString("yyyy-MM-dd"), // Format date correctly
                    Status = x.Status,
                    x.CreatedBy,
                    x.ModifiedBy
                }).ToList();

                if (data.Count == 0)
                {
                    return Json(new { error = "No users found for the given date range." });
                }

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Server Error", details = ex.Message });
            }
        }



        [HttpGet]
        public IActionResult FetchEmp()
        {
            // Get the current user's username
            var username = User.Identity.Name;

            // Query the database for users created by the logged-in user
            var data = db.User
                .Include(u => u.Role).Where(x => x.Role.RoleName != "Admin")
                .Include(u => u.Department)
                .Include(u => u.Designation)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PasswordHash,
                    u.PhoneNumber,
                    u.Role.RoleName,
                    //Role = new { RoleName = u.Role.RoleName },

                    Department = new { Name = u.Department.Name },
                    Designation = new { Name = u.Designation.Name },
                    u.ReportingManager,
                    u.DateOfBirth,
                    u.DateOfJoining,
                    u.Address,
                    u.Gender,
                    u.Status,
                    u.CreatedBy,
                    u.ModifiedBy,
                    u.AboutEmployee,
                    u.ProfilePicture
                })
                .ToList();

            ViewBag.Roles = db.Role.ToList();

            //var roles = db.Role.Where(x => x.Status.Equals("Active"))
            //.Select(d => new { roleId = d.RoleId, roleName = d.RoleName })
            //.ToList();

            //return Json(new { success = true, data = udata, roles });

            // Return the filtered data as JSON
            return Json(data);


        }


        public IActionResult EditEmp(int eid)
        {
            var us = db.User.Find(eid);
            if (us == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Get active departments
            var users = db.Role
                                .Where(x => x.Status.Equals("Active"))
                                .Select(d => new { roleId = d.RoleId, roleName = d.RoleName })
                                .ToList();

            // Default status options
            //List<string> statuses = new List<string> { "Active", "Inactive" };

            return Json(new { success = true, data = us, users });
        }


        //[HttpGet]
        //public IActionResult FetchEmp()
        //{
        //    var data = db.User
        //        .Include(u => u.Role).Where(x => x.Role.RoleName != "Admin")
        //        .Include(u => u.Department)
        //        .Include(u => u.Designation)
        //        .Select(u => new
        //        {
        //            u.UserId,
        //            u.FirstName,
        //            u.LastName,
        //            u.Email,
        //            u.PasswordHash,
        //            u.PhoneNumber,
        //            u.RoleId, // ✅ Include RoleId here
        //            RoleName = u.Role.RoleName, // ✅ Keep RoleName for display
        //            DepartmentId = u.Department.DepartmentId,
        //            DesignationId = u.Designation.DesignationId,
        //            u.ReportingManager,
        //            u.DateOfBirth,
        //            u.DateOfJoining,
        //            u.Address,
        //            u.Gender,
        //            u.Status,
        //            u.CreatedBy,
        //            u.ModifiedBy,
        //            u.AboutEmployee,
        //            u.ProfilePicture
        //        })
        //        .ToList();

        //    ViewBag.Roles = db.Role.Select(r => new { r.RoleId, r.RoleName }).ToList(); // ✅ Send Roles list

        //    return Json(new { success = true, users = data, roles = ViewBag.Roles });
        //}

        [HttpPost]
        public IActionResult AddEmp(User u, IFormFile ProfilePicture)
        {

            if (u.Role != null && u.Role.RoleName == "Admin")
            {
                u.DepartmentId = (u.DepartmentId == -1) ? (int?)null : u.DepartmentId;
                u.DesignationtId = (u.DesignationtId == -1) ? (int?)null : u.DesignationtId;
            }

            if (ProfilePicture != null)
            {
                // Allowed image MIME types
                var allowedTypes = new List<string> { "image/jpeg", "image/png", "image/gif", "image/jpg" };
                var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };

                var fileExtension = Path.GetExtension(ProfilePicture.FileName).ToLower();

                if (!allowedTypes.Contains(ProfilePicture.ContentType) || !allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Only image files (JPEG, PNG, GIF) are allowed." });
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Content/uploads", ProfilePicture.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    ProfilePicture.CopyTo(stream);
                }
                u.ProfilePicture = "Content/uploads/" + ProfilePicture.FileName;
            }
            string email = HttpContext.Session.GetString("Email");
            var user = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

            if (user != null)
            {
                u.CreatedBy = user.FirstName;  // Assign FirstName to CreatedBy
                u.CreatedAt = DateTime.UtcNow; // Set CreatedAt timestamp
            }
            db.User.Add(u);
            db.SaveChanges();

            // New Logic: Assign department-wise leaves to the newly registered employee
            if (u.DepartmentId.HasValue)
            {
                var departmentLeaves = db.DepartmentLeaves.Where(dl => dl.DepartmentId == u.DepartmentId).ToList();
                foreach (var deptLeave in departmentLeaves)
                {
                    var leaveBalance = new LeaveBalance
                    {
                        UserId = u.UserId, // Newly registered user ID
                        DepartmentLeavesId = deptLeave.DepartmentLeavesId,
                        LeaveTypeId = deptLeave.LeaveTypeId,
                        TotalLeaves = deptLeave.LeavesCount,
                        UsedLeaves = 0
                    };
                    db.LeaveBalances.Add(leaveBalance);
                }
                db.SaveChanges(); // Save leave balances for new user
            }
            return Json("");
        }

        public IActionResult EditUser(User u, IFormFile ProfilePicture)
        {
            var user = db.User.FirstOrDefault(x => x.UserId == u.UserId);
            if (user != null)
            {

                if (u.Role != null && u.Role.RoleName == "Admin")
                {
                    user.DepartmentId = (u.DepartmentId == -1) ? (int?)null : u.DepartmentId;
                    user.DesignationtId = (u.DesignationtId == -1) ? (int?)null : u.DesignationtId;

                }

                if (ProfilePicture != null)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Content/uploads");
                    // Ensure the directory exists
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // Generate a unique file name to avoid conflicts (e.g., using GUID)
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePicture.FileName);
                    var path = Path.Combine(uploadDir, uniqueFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        ProfilePicture.CopyTo(stream);
                    }
                    user.ProfilePicture = "Content/uploads/" + uniqueFileName;
                }
                user.FirstName = u.FirstName;
                user.LastName = u.LastName;
                user.Email = u.Email;
                user.PasswordHash = u.PasswordHash;
                user.PhoneNumber = u.PhoneNumber;
                user.Role = u.Role;
                //user.DepartmentId = u.DepartmentId;
                //user.DesignationId = u.DesignationId;
                user.DepartmentId = (u.DepartmentId == null || u.DepartmentId == 0) ? user.DepartmentId : u.DepartmentId;
                user.DesignationtId = (u.DesignationtId == null || u.DesignationtId == 0) ? user.DesignationtId : u.DesignationtId;
                user.ReportingManager = u.ReportingManager;
                user.DateOfJoining = u.DateOfJoining;
                user.Status = u.Status;
                user.DateOfBirth = u.DateOfBirth;
                user.Gender = u.Gender;
                user.Address = u.Address;
                user.AboutEmployee = u.AboutEmployee;
                user.ReportingManager = u.ReportingManager;

                var managers = db.User
          .Where(e => e.RoleId == 2)
          .Select(e => new
          {
              Value = e.FirstName.ToString(),
              Text = e.FirstName + " " + e.LastName,
              DepartmentId = e.DepartmentId
          })
          .ToList();


                // Pass managers to the view
                ViewBag.Managers = managers;

                string email = HttpContext.Session.GetString("Email");
                var username = db.User.FirstOrDefault(u => u.Email == email); // Find user by email

                if (username != null)
                {
                    user.ModifiedBy = username.FirstName;  // Assign FirstName to ModifiedBy
                    user.ModifiedAt = DateTime.UtcNow;
                }


                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Department not found." });
            }
        }

        public IActionResult DeleteUser(int id)
        {
            var user = db.User.FirstOrDefault(x => x.UserId == id);
            if (user != null)
            {
                db.User.Remove(user);
                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Department not found." });
            }
        }
		//grid showing
		//public IActionResult EmployeeGrid()
		//{
		//    var users = db.User
		//        .Where(u => u.Role.RoleName != "Admin")
		//    .Include(u => u.Designation)
		//    .Include(u => u.Projects)
		//    .ThenInclude(p => p.Tasks)
		//    .Select(u => new
		//    {
		//        u.UserId,
		//        u.FirstName,
		//        u.LastName,
		//        u.ProfilePicture,
		//        DesignationName = u.Designation != null ? u.Designation.Name : "N/A",
		//        TotalProjects = u.Projects.Count(), // Fetch from Many-to-Many table
		//        CompletedTasks = u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Completed"),
		//        InProgressTasks = u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Inprogress"),

		//        Productivity = u.Projects.SelectMany(p => p.Tasks).Count() > 0
		//        ? (u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Completed") * 100)
		//        / u.Projects.SelectMany(p => p.Tasks).Count()
		//        : 0
		//    })
		//    .ToList();



		//    ViewBag.Roles = new SelectList(db.Role.ToList(), "RoleId", "RoleName");
		//    ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentId", "Name");
		//    ViewBag.Designations = new SelectList(db.Designations.ToList(), "DesignationId", "Name");
		//    ViewBag.ActiveMemberCount = db.User.Count(m => m.Status == "Active");
		//    ViewBag.InactiveMemberCount = db.User.Count(m => m.Status == "Inactive");
		//    ViewBag.TotalMemberCount = db.User.Count();
		//    ViewBag.RecentlyAddedCount = db.User.Count(m => m.DateOfJoining >= DateTime.Now.AddDays(-7));
		//    return View(users);
		//}
		public IActionResult EmployeeGrid()
		{
			var managerRoleId = db.Role.Where(r => r.RoleName == "Manager").Select(r => r.RoleId).FirstOrDefault();

			// Fetch users where RoleId matches the Manager role
			var managers = db.User.Where(e => e.RoleId == managerRoleId)
				.Select(e => new
				{
					Value = e.FirstName.ToString(),
					Text = e.FirstName + " " + e.LastName,
					DepartmentId = e.DepartmentId // Include DepartmentId for filtering
				})
				.ToList();

			ViewBag.Managers = managers;


			var users = db.User
				.Where(u => u.Role.RoleName != "Admin")
			.Include(u => u.Designation)
			.Include(u => u.Projects)
			.ThenInclude(p => p.Tasks)
			.Select(u => new
			{
				u.UserId,
				u.FirstName,
				u.LastName,
				u.ProfilePicture,
				DesignationId = u.Designation != null ? u.Designation.DesignationId : 0, // Include DesignationId
				DesignationName = u.Designation != null ? u.Designation.Name : "N/A",
				TotalProjects = u.Projects.Count(), // Fetch from Many-to-Many table
				CompletedTasks = u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Completed"),
				InProgressTasks = u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Inprogress"),

				Productivity = u.Projects.SelectMany(p => p.Tasks).Count() > 0
				? (u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Completed") * 100)
				/ u.Projects.SelectMany(p => p.Tasks).Count()
				: 0
			})
			.ToList();
			ViewBag.Designation = db.Designations
				.Where(d => d.status == "Active")
				.Select(d => new SelectListItem
				{
					Value = d.DesignationId.ToString(),
					Text = d.Name
				})
				.ToList();

			// Fetch active roles
			ViewBag.Roles = new SelectList(
				db.Role.Where(r => r.Status == "Active").ToList(), "RoleId", "RoleName"
			);

			// Fetch active departments
			ViewBag.Departments = new SelectList(
				db.Departments.Where(d => d.Status == "Active").ToList(), "DepartmentId", "Name"
			);

			// Fetch active designations for dropdown
			ViewBag.Designations = new SelectList(
				db.Designations.Where(d => d.status == "Active").ToList(), "DesignationId", "Name"
			);



			//ViewBag.Roles = new SelectList(db.Role.ToList(), "RoleId", "RoleName");
			//ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentId", "Name");
			//ViewBag.Designations = new SelectList(db.Designations.ToList(), "DesignationId", "Name");
			//ViewBag.ActiveMemberCount = db.User.Count(m => m.Status == "Active");
			//ViewBag.InactiveMemberCount = db.User.Count(m => m.Status == "Inactive");
			//ViewBag.TotalMemberCount = db.User.Count();
			//ViewBag.RecentlyAddedCount = db.User.Count(m => m.DateOfJoining >= DateTime.Now.AddDays(-7));

			//var users = db.User.Include(u => u.Designation).ToList(); // Fetch users with designation
			//ViewBag.Roles = new SelectList(db.Role.ToList(), "RoleId", "RoleName");
			//ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentId", "Name");
			//ViewBag.Designations = new SelectList(db.Designations.ToList(), "DesignationId", "Name");
			ViewBag.ActiveMemberCount = db.User.Count(m => m.Status == "Active");
			ViewBag.InactiveMemberCount = db.User.Count(m => m.Status == "Inactive");
			ViewBag.TotalMemberCount = db.User.Count();
			ViewBag.RecentlyAddedCount = db.User.Count(m => m.DateOfJoining >= DateTime.Now.AddDays(-7));


			return View(users);
		}
		public IActionResult SortUserGr(string sortBy)
		{
			var users = db.User
				.Where(u => u.Role.RoleName != "Admin")
				.Include(u => u.Designation)
				.Include(u => u.Projects)
				.ThenInclude(p => p.Tasks)
				.Select(u => new
				{
					u.UserId,
					u.FirstName,
					u.LastName,
					u.ProfilePicture,
					DesignationId = u.Designation != null ? u.Designation.DesignationId : 0,
					DesignationName = u.Designation != null ? u.Designation.Name : "N/A",
					TotalProjects = u.Projects.Count(),
					CompletedTasks = u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Completed"),
					InProgressTasks = u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Inprogress"),
					Productivity = u.Projects.SelectMany(p => p.Tasks).Count() > 0
						? (u.Projects.SelectMany(p => p.Tasks).Count(t => t.Status == "Completed") * 100)
						  / u.Projects.SelectMany(p => p.Tasks).Count()
						: 0
				}).ToList();

			switch (sortBy)
			{
				case "asc":
					//users = users.OrderBy(u => u.FirstName).ToList();
					users = users.OrderBy(u => u.UserId).ToList();
					break;
				case "desc":
					//users = users.OrderByDescending(u => u.FirstName).ToList();
					users = users.OrderByDescending(u => u.UserId).ToList();
					break;
				case "productivityasc":
					users = users.OrderBy(u => u.Productivity).ToList();
					break;
				case "productivitydesc":
					users = users.OrderByDescending(u => u.Productivity).ToList();
					break;
				case "last7days":
					users = users.Where(u => db.User.Any(m => m.DateOfJoining >= DateTime.Now.AddDays(-7))).ToList();
					break;
				case "thismonth":
					users = users.Where(u => db.User.Any(m => m.DateOfJoining.Month == DateTime.Now.Month &&
															  m.DateOfJoining.Year == DateTime.Now.Year)).ToList();
					break;
				case "thisyear":
					users = users.Where(u => db.User.Any(m => m.DateOfJoining.Year == DateTime.Now.Year)).ToList();
					break;
			}

			return Json(users);
		}

		public IActionResult SearchUser(string mydata)
        {
            if (mydata != null)
            {
                var data = db.User.Where(x => x.FirstName.Contains(mydata) || x.LastName.Contains(mydata) || x.Email.Contains(mydata) || x.PhoneNumber.Contains(mydata)).ToList();
                return Json(data);
            }
            else
            {
                var data = db.User.ToList();
                return Json(data);
            }
        }

        //public IActionResult SortingByAsc(string mydata)
        //{
        //    if (mydata.Equals("asc"))
        //    {
        //        var data = db.User.OrderBy(x => x.UserId).ToList();
        //        return Json(data);
        //    }
        //    else if (mydata.Equals("desc"))
        //    {
        //        var data = db.User.OrderByDescending(x => x.UserId).ToList();
        //        return Json(data);
        //    }
        //    else
        //    {
        //        var data = db.User.ToList();
        //        return Json(data);
        //    }
        //}


        public IActionResult SortUser(string sortBy)
        {
            try
            {
                var query = db.User
                    .Include(x => x.Designation)
                    .Include(x => x.Department)
                    .Include(x => x.Role).Where(u => u.Role.RoleName != "Admin")
                    .AsQueryable();

                DateTime today = DateTime.Today;
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime firstDayOfYear = new DateTime(today.Year, 1, 1);
                DateTime sevenDaysAgo = today.AddDays(-7);

                switch (sortBy.ToLower())
                {
                    case "asc":
                        query = query.OrderBy(x => x.UserId);
                        break;
                    case "desc":
                        query = query.OrderByDescending(x => x.UserId);
                        break;
                    case "last7days":
                        query = query.Where(x => x.DateOfJoining >= sevenDaysAgo);
                        break;
                    case "thismonth":
                        query = query.Where(x => x.DateOfJoining >= firstDayOfMonth);
                        break;
                    case "thisyear":
                        query = query.Where(x => x.DateOfJoining >= firstDayOfYear);
                        break;
                    default:
                        query = query.OrderBy(x => x.UserId); // Default sorting
                        break;
                }

                var data = query.Select(x => new
                {
                    UserId = x.UserId,
                    ProfilePicture = x.ProfilePicture,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    DesignationName = x.Designation != null ? x.Designation.Name : "N/A",
                    ReportingManager = x.ReportingManager,
                    DateOfJoining = x.DateOfJoining.ToString("yyyy-MM-dd"), // Format date correctly
                    Status = x.Status,
                    x.CreatedBy,
                    x.ModifiedBy
                }).ToList();

                if (data.Count == 0)
                {
                    return Json(new { error = "No users found for the selected criteria." });
                }

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Server Error", details = ex.Message });
            }
        }


		//public IActionResult SortingUserStatus(string mydata)
		//{
		//    if (!string.IsNullOrEmpty(mydata))
		//    {
		//        var data = db.User.Where(x => x.Status == mydata).ToList();
		//        return Json(data);
		//    }
		//    else
		//    {
		//        var data = db.User.ToList();
		//        return Json(data);
		//    }
		//}


		//public IActionResult SortingUserStatus(string status)
		//{
		//    try
		//    {
		//        if (!string.IsNullOrEmpty(status))
		//        {
		//            var data = db.User
		//                .Include(x => x.Designation)
		//                .Include(x => x.Department)
		//                .Include(x => x.Role)
		//                .Where(x => x.Status == status) // Filter by status
		//                .Select(x => new
		//                {
		//                    UserId = x.UserId,
		//                    ProfilePicture = x.ProfilePicture,
		//                    FirstName = x.FirstName,
		//                    LastName = x.LastName,
		//                    Email = x.Email,
		//                    PhoneNumber = x.PhoneNumber,
		//                    DesignationName = x.Designation != null ? x.Designation.Name : "N/A",
		//                    ReportingManager = x.ReportingManager,
		//                    DateOfJoining = x.DateOfJoining.ToString("yyyy-MM-dd"), // Format date correctly
		//                    Status = x.Status,
		//                    x.CreatedBy,
		//                    x.ModifiedBy
		//                })
		//                .ToList();

		//            if (data.Count == 0)
		//            {
		//                return Json(new { error = "No users found for the selected status." });
		//            }

		//            return Json(data);
		//        }

		//        return Json(new { error = "Invalid status selection." });
		//    }
		//    catch (Exception ex)
		//    {
		//        return Json(new { error = "Server Error", details = ex.Message });
		//    }
		//}
		public IActionResult SortingUserStatus(string status)
		{
			try
			{
				if (status == "select")
				{
					// Return all users when "select" is chosen
					var data = db.User
						.Include(x => x.Designation)
						.Include(x => x.Department)
						.Include(x => x.Role).Where(u => u.Role.RoleName != "Admin")
                        .Select(x => new
						{
							UserId = x.UserId,
							ProfilePicture = x.ProfilePicture,
							FirstName = x.FirstName,
							LastName = x.LastName,
							Email = x.Email,
							PhoneNumber = x.PhoneNumber,
							DesignationName = x.Designation != null ? x.Designation.Name : "N/A",
							ReportingManager = x.ReportingManager,
							DateOfJoining = x.DateOfJoining.ToString("yyyy-MM-dd"), // Format date correctly
							Status = x.Status,
							x.CreatedBy,
							x.ModifiedBy
						})
						.ToList();

					return Json(data);
				}
				else if (!string.IsNullOrEmpty(status))
				{
					// Filter users based on the selected status
					var data = db.User
						.Include(x => x.Designation)
						.Include(x => x.Department)
						.Include(x => x.Role)
                        .Where(x => x.Status == status && x.Role.RoleName != "Admin") // Filter by status
						.Select(x => new
						{
							UserId = x.UserId,
							ProfilePicture = x.ProfilePicture,
							FirstName = x.FirstName,
							LastName = x.LastName,
							Email = x.Email,
							PhoneNumber = x.PhoneNumber,
							DesignationName = x.Designation != null ? x.Designation.Name : "N/A",
							ReportingManager = x.ReportingManager,
							DateOfJoining = x.DateOfJoining.ToString("yyyy-MM-dd"), // Format date correctly
							Status = x.Status,
							x.CreatedBy,
							x.ModifiedBy
						})
						.ToList();

					if (data.Count == 0)
					{
						return Json(new { error = "No users found for the selected status." });
					}

					return Json(data);
				}
				else
				{
					// Default case: return an error for invalid status selection
					return Json(new { error = "Invalid status selection." });
				}
			}
			catch (Exception ex)
			{
				// Handle server errors
				return Json(new { error = "Server Error", details = ex.Message });
			}
		}


		public IActionResult GetUserEntries(int count)
        {
            var data = db.User.Where(u => u.Role.RoleName != "Admin").Take(count).ToList(); // Get only the specified number of entries
            return Json(data);
        }

        //User Personal Profile start
        public IActionResult UserProfile()
        {
            //ViewBag.FemilyUID = new SelectList(db.User, "Userid", "FirstName");

            string email = HttpContext.Session.GetString("Email");
            string role = HttpContext.Session.GetString("Role");
            string Path = HttpContext.Session.GetString("Epath");



            // var user = db.User.FirstOrDefault(p => p.Email == email);
            var user = db.User.Include(u => u.Role).Include(u => u.Department).Include(u => u.Designation).FirstOrDefault(p => p.Email == email);

            var familyDetails = db.EmployeeFamilyDetails.Where(f => f.UserId == user.UserId).ToList();
            //var family = db.EmployeeFamilyDetails.FirstOrDefault(f => f.User.UserId == email);
            var bankDetails = db.EmployeeBankDetails.Where(f => f.UserId == user.UserId).ToList();
            //education
            var eduDetails = db.EducationDetails.Where(f => f.UserId == user.UserId).ToList();
            //experianc
            var exDetails = db.Experience.Where(f => f.UserId == user.UserId).ToList();
            ViewBag.UserId = user.UserId;
            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.Email = email;
            ViewBag.PhoneNumber = user.PhoneNumber;
            //ViewBag.Role =user.Role.RoleName;
            ViewBag.Role = user.Role.RoleName;
            //ViewBag.Department = user.Department.Name;
            //ViewBag.Designation = user.Designation.Name;
            ViewBag.Department = user.Department?.Name ?? "Not Applicable";
            ViewBag.Designation = user.Designation?.Name ?? "Not Applicable";
            ViewBag.Path = Path;
            ViewBag.DOB = user.DateOfBirth;
            ViewBag.DOJ = user.DateOfJoining;
            ViewBag.Add = user.Address;
            ViewBag.Gender = user.Gender;
            ViewBag.Status = user.Status;
            ViewBag.ReportManager = user.ReportingManager;
            ViewBag.About = user.AboutEmployee;


            //family viewbag
            ViewBag.FamilyDetails = familyDetails;
            ViewBag.BankDetails = bankDetails;
            ViewBag.EducationDetails = eduDetails;
            ViewBag.ExperianceDetails = exDetails;
            return View();
        }


        public IActionResult FetchUserDetails(int id)
        {
            //var familyDetail = db.EmployeeFamilyDetails.FirstOrDefault(fd => fd.FamilyDetailId == id);
            var user = db.User.Include(u => u.Role).FirstOrDefault(u => u.UserId == id);

            if (user != null)
            {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        user.UserId,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        RoleName = user.Role.RoleName, // Include the role name directly
                        user.PhoneNumber,
                        user.Department,
                        user.Designation,
                        user.DateOfBirth,
                        user.DateOfJoining,
                        user.Address,
                        user.Gender,
                        user.Status,
                        user.AboutEmployee
                    }
                });
            }

            return Json(new { success = false, message = "User not found." });
        }


        [HttpPost]
        public IActionResult UpdateProfile([FromForm] User pro, IFormFile ProfilePicture)
        {
            try
            {
                if (pro == null)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                // Fetch the user from the database
                var user = db.User.FirstOrDefault(u => u.UserId == pro.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Update basic fields
                user.FirstName = pro.FirstName;
                user.LastName = pro.LastName;
                user.Email = pro.Email;
                user.PhoneNumber = pro.PhoneNumber;
                user.Address = pro.Address;
                user.AboutEmployee = pro.AboutEmployee;
                user.DateOfBirth = pro.DateOfBirth;

                // Handle profile picture upload
                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    // Validate file type (e.g., only allow image files)
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ProfilePicture.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Json(new { success = false, message = "Invalid file type." });
                    }

                    // Generate a unique file name
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ProfilePicture.CopyTo(stream);
                    }

                    user.ProfilePicture = $"/uploads/{uniqueFileName}"; // Save relative path
                }

                // Save changes to the database
                db.SaveChanges();

                return Json(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                // Log the error if needed
                return Json(new { success = false, message = "An error occurred.", error = ex.Message });
            }
        }

        //start with Family Details
        public IActionResult AddFamilyDetail(EmployeeFamilyDetail efd)
        {
            db.EmployeeFamilyDetails.Add(efd);
            db.SaveChanges();
            return Json("");
        }
        public IActionResult EditFamilyDetail(EmployeeFamilyDetail efd)
        {
            var eefd = db.EmployeeFamilyDetails.FirstOrDefault(x => x.UserId == efd.UserId);
            if (eefd != null)
            {
                eefd.Name = efd.Name;
                eefd.Relation = efd.Relation;
                eefd.DateOfBirth = efd.DateOfBirth;
                eefd.phone = efd.phone;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Department not found." });
        }

        //[HttpGet("Employee/FetchFamilyDetails/{id}")]

        public IActionResult FindFamilyDetails(int id)
        {
            //var familyDetail = db.EmployeeFamilyDetails.Where(x => x.UserId.Equals(id)).SingleOrDefault();
            var familyDetail = db.EmployeeFamilyDetails.Find(id);
            return Json(familyDetail);

            //if (familyDetail != null)
            //{
            //    //return Json(new { success = true, data = familyDetail });
            //    return Json(familyDetail);
            //}
            //else
            //{
            //    //return Json(new { success = false, message = "Family detail not found." });
            //    return NotFound();
            //}

        }

        //public IActionResult FetchFamilyDetails(int id)
        //{
        //    Console.WriteLine($"[Debug] Received Family ID: {id}"); // Add this for debugging
        //    //var familyDetail = db.EmployeeFamilyDetails
        //    //         .AsNoTracking() // Disable change tracking if not needed
        //    //         .FirstOrDefault(fd => fd.UserId == id);
        //    var familyDetail = db.EmployeeFamilyDetails.FirstOrDefault(fd => fd.UserId == id);

        //    if (familyDetail != null)
        //    {
        //        Console.WriteLine("[Debug] Family detail found.");
        //        return Json(new { success = true, data = familyDetail });
        //    }

        //    Console.WriteLine("[Debug] Family detail not found.");
        //    return Json(new { success = false, message = "Family detail not found." });
        //}
        [HttpPost]
        public IActionResult UpdateFamilyDetails(EmployeeFamilyDetail familyDetail)
        {
            //var existingFamilyDetail = db.EmployeeFamilyDetails.FirstOrDefault(fd => fd.FamilyDetailId == familyDetail.FamilyDetailId);
            //if (existingFamilyDetail != null)
            //{
            //    existingFamilyDetail.Name = familyDetail.Name;
            //    existingFamilyDetail.Relation = familyDetail.Relation;
            //    existingFamilyDetail.DateOfBirth = familyDetail.DateOfBirth;
            //    existingFamilyDetail.phone = familyDetail.phone;

            //    db.SaveChanges();
            //    return Json(new { success = true, data = existingFamilyDetail });
            //}
            //return Json(new { success = false, message = "Family detail not found." });



            db.EmployeeFamilyDetails.Update(familyDetail);
            db.SaveChanges();
            return new JsonResult("");
        }
        public IActionResult AddBankDetail(EmployeeBankDetails bank)
        {
            db.EmployeeBankDetails.Add(bank);
            db.SaveChanges();
            return Json("");
        }
        public IActionResult FindBankDetails(int id)
        {
            var bankDetail = db.EmployeeBankDetails.Find(id);
            return Json(bankDetail);
        }
        public IActionResult UpdateBankDetails(EmployeeBankDetails BankDetail)
        {
            db.EmployeeBankDetails.Update(BankDetail);
            db.SaveChanges();
            return new JsonResult("");
        }

        public IActionResult AddEducationDetail(EducationDetails edu)
        {
            db.EducationDetails.Add(edu);
            db.SaveChanges();
            return Json("");
        }
        public IActionResult FindEducationDetails(int id)
        {
            var eduDetail = db.EducationDetails.Find(id);
            return Json(eduDetail);
        }
        public IActionResult UpdateEducationDetails(EducationDetails edu)
        {
            db.EducationDetails.Update(edu);
            db.SaveChanges();
            return new JsonResult("");
        }
        //experiance
        public IActionResult AddExperianceDetail(Experience ex)
        {
            db.Experience.Add(ex);
            db.SaveChanges();
            return Json("");
        }
        public IActionResult FindExperienceDetails(int id)
        {
            var ex = db.Experience.Find(id);
            return Json(ex);
        }
        public IActionResult UpdateExperienceDetails(Experience ex)
        {
            db.Experience.Update(ex);
            db.SaveChanges();
            return new JsonResult("");
        }

        public IActionResult ExportToPdf()
        {
            try
            {
                var users = db.User
                    .Include(u => u.Designation)
                    .ToList();

                MemoryStream workStream = new MemoryStream();
                Document document = new Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, workStream);
                document.Open();

                // Define fonts
                iTextSharp.text.Font fontHeader = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 12);
                iTextSharp.text.Font fontContent = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10);

                PdfPTable table = new PdfPTable(10); // Number of columns
                table.WidthPercentage = 100;

                // Table headers
                table.AddCell(new PdfPCell(new Phrase("Id", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Name", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Email", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Number", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Designation", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Reporting Manager", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("DOJ", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Status", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Created By", fontHeader)));
                table.AddCell(new PdfPCell(new Phrase("Modified By", fontHeader)));

                // Table content
                foreach (var user in users)
                {
                    table.AddCell(new PdfPCell(new Phrase(user.UserId.ToString(), fontContent)));
                    table.AddCell(new PdfPCell(new Phrase($"{user.FirstName} {user.LastName}", fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.Email, fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.PhoneNumber, fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.Designation.Name ?? "N/A", fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.ReportingManager ?? "N/A", fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.DateOfJoining.ToString("yyyy-MM-dd"), fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.Status, fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.CreatedBy ?? "N/A", fontContent)));
                    table.AddCell(new PdfPCell(new Phrase(user.ModifiedBy ?? "N/A", fontContent)));
                }

                document.Add(table);
                document.Close();

                byte[] byteArray = workStream.ToArray();
                workStream.Close();
                return File(byteArray, "application/pdf", "UsersList.pdf");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        public IActionResult ExportToExcel()
        {
            var users = db.User
                .Include(u => u.Designation)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users List");
                var currentRow = 1;

                // Headers
                worksheet.Cell(currentRow, 1).Value = "Id";
                worksheet.Cell(currentRow, 2).Value = "Name";
                worksheet.Cell(currentRow, 3).Value = "Email";
                worksheet.Cell(currentRow, 4).Value = "Number";
                worksheet.Cell(currentRow, 5).Value = "Designation";
                worksheet.Cell(currentRow, 6).Value = "Reporting Manager";
                worksheet.Cell(currentRow, 7).Value = "DOJ";
                worksheet.Cell(currentRow, 8).Value = "Status";
                worksheet.Cell(currentRow, 9).Value = "Created By";
                worksheet.Cell(currentRow, 10).Value = "Modified By";

                // Content
                foreach (var user in users)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = user.UserId;
                    worksheet.Cell(currentRow, 2).Value = $"{user.FirstName} {user.LastName}";
                    worksheet.Cell(currentRow, 3).Value = user.Email;
                    worksheet.Cell(currentRow, 4).Value = user.PhoneNumber;
                    worksheet.Cell(currentRow, 5).Value = user.Designation?.Name ?? "N/A";
                    worksheet.Cell(currentRow, 6).Value = user.ReportingManager ?? "N/A";
                    worksheet.Cell(currentRow, 7).Value = user.DateOfJoining.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 8).Value = user.Status;
                    worksheet.Cell(currentRow, 9).Value = user.CreatedBy ?? "N/A";
                    worksheet.Cell(currentRow, 10).Value = user.ModifiedBy ?? "N/A";
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UsersList.xlsx");
                }
            }
        }
    }
}
