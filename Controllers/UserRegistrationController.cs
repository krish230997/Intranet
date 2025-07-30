using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
	public class UserRegistrationController : Controller
	{
		private readonly ApplicationDbContext db;
		public UserRegistrationController(ApplicationDbContext db) 
		{
			this.db = db;
		}
		public IActionResult AddOrganisation()
		{
			return View();

		}
		[HttpPost]
		public IActionResult AddOrganisation(Organization org, IFormFile OrganizationLogoFile)
		{

			// Check if the file is uploaded
			if (OrganizationLogoFile != null && OrganizationLogoFile.Length > 0)
			{
				// Validate file type
				var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
				var fileExtension = Path.GetExtension(OrganizationLogoFile.FileName).ToLower();

				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("OrganizationLogo", "Only PNG and JPG files are allowed.");
					return View(org);
				}

				// Save the file
				var fileName = $"{Guid.NewGuid()}{fileExtension}";
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos", fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					OrganizationLogoFile.CopyTo(stream);
				}

				// Store the file path in the database
				org.OrganizationLogo = $"/uploads/logos/{fileName}";
			}

			db.Organization.Add(org);
			db.SaveChanges();
			return RedirectToAction("FetchOraganisation");


			// return View(org);
		}


		public IActionResult FetchOraganisation()
		{
			var data = db.Organization.ToList();
			return View(data);
		}


		public IActionResult EditOrganisation(int id)
		{
			var organization = db.Organization.Find(id);
			if (organization == null)
			{
				return NotFound();
			}
			return View(organization);
		}

		[HttpPost]
		public IActionResult EditOrganisation(Organization org)
		{
			if (ModelState.IsValid)
			{
				var existingOrg = db.Organization.Find(org.OrganizationId); // Fetch from DB
				if (existingOrg == null)
				{
					return NotFound();
				}


				existingOrg.OrganizationName = org.OrganizationName;
				existingOrg.OrganizationDescription = org.OrganizationDescription;

				db.SaveChanges();
				return RedirectToAction("FetchOraganisation");
			}
			return View(org);
		}
		[HttpPost]
		public IActionResult DeleteOrganisation(int id)
		{
			var organization = db.Organization.Find(id);
			if (organization != null)
			{
				db.Organization.Remove(organization);
				db.SaveChanges();
			}
			return RedirectToAction("FetchOraganisation");
		}

		public IActionResult FetchDesignations()
		{
			var data = db.Designations.ToList();
			return View(data);
		}


		public IActionResult AddDesignation()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AddDesignation(Designation desig)
		{
			
				db.Designations.Add(desig);
				db.SaveChanges();
				return RedirectToAction("FetchDesignations");
			
			return View(desig);
		}

		public IActionResult EditDesignation(int id)
		{
			var designation = db.Designations.Find(id);
			if (designation == null)
			{
				return NotFound();
			}
			return View(designation);
		}

		[HttpPost]
		public IActionResult EditDesignation(Designation desig)
		{
			
				var existingDesig = db.Designations.Find(desig.DesignationId);
				if (existingDesig == null)
				{
					return NotFound();
				}


				existingDesig.Name = desig.Name;

				db.SaveChanges();
			return RedirectToAction("FetchDesignations");
			
			return View(desig);
		}


		[HttpPost]
		public IActionResult DeleteDesignation(int id)
		{
			var designation = db.Designations.Find(id);
			if (designation != null)
			{
				db.Designations.Remove(designation);
				db.SaveChanges();
			}
			return RedirectToAction("FetchDesignations");
		}
		public IActionResult FetchRoles()
		{
			var data = db.Role.ToList(); // Fetch all roles
			return View(data);
		}

		// Add Role
		public IActionResult AddRole()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AddRole(Role role)
		{
			
				//role.CreatedAt = DateTime.Now; // Set CreatedAt
				db.Role.Add(role); // Add role to DB
				db.SaveChanges(); // Save changes
				return RedirectToAction("FetchRoles");
			
			return View(role);
		}

		// Edit Role
		public IActionResult EditRole(int id)
		{
			var role = db.Role.Find(id); // Find role by ID
			if (role == null)
			{
				return NotFound();
			}
			return View(role);
		}

		[HttpPost]
		public IActionResult EditRole(Role role)
		{
			
				var existingRole = db.Role.Find(role.RoleId); // Fetch from DB
				if (existingRole == null)
				{
					return NotFound();
				}

				// Update fields
				existingRole.RoleName = role.RoleName;
				existingRole.Status = role.Status;

				db.SaveChanges(); // Commit changes to DB
				return RedirectToAction("FetchRoles");
			
			return View(role);
		}

		// Delete Role
		[HttpPost]
		public IActionResult DeleteRole(int id)
		{
			var role = db.Role.Find(id); // Find by ID
			if (role != null)
			{
				db.Role.Remove(role); // Remove from DB
				db.SaveChanges(); // Commit changes
			}
			return RedirectToAction("FetchRoles");
		}

		// Fetch all Departments
		public IActionResult FetchDepartments()
		{
			var data = db.Departments.ToList();
			return View(data);
		}

		// Add Department
		public IActionResult AddDepartment()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AddDepartment(Department dept)
		{
			
				
				db.Departments.Add(dept);
				db.SaveChanges();
				return RedirectToAction("FetchDepartments");
			
			return View(dept);
		}

		// Edit Department
		public IActionResult EditDepartment(int id)
		{
			var department = db.Departments.Find(id);
			if (department == null)
			{
				return NotFound();
			}
			return View(department);
		}

		[HttpPost]
		public IActionResult EditDepartment(Department dept)
		{
			if (ModelState.IsValid)
			{
				var existingDept = db.Departments.Find(dept.DepartmentId);
				if (existingDept == null)
				{
					return NotFound();
				}

				// Update fields
				existingDept.Name = dept.Name;
				existingDept.Status = dept.Status;
				

				db.SaveChanges();
				return RedirectToAction("FetchDepartments");
			}
			return View(dept);
		}

		// Delete Department
		[HttpPost]
		public IActionResult DeleteDepartment(int id)
		{
			var department = db.Departments.Find(id);
			if (department != null)
			{
				db.Departments.Remove(department);
				db.SaveChanges();
			}
			return RedirectToAction("FetchDepartments");
		}



	}
}
