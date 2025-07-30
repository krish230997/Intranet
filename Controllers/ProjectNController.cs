using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using Pulse360.Data;
using Pulse360.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace Pulse360.Controllers
{
	public class ProjectNController : Controller
	{
		private readonly ApplicationDbContext db;

		public ProjectNController(ApplicationDbContext db)
		{
			this.db = db;
		}
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult ProjectGrid()
		{
			//var projects = db.AllProjects.ToList(); // Fetch all projects from the database
			//return View(projects); // Pass projects to the view
			//    var projects = db.AllProjects
			//.Select(p => new
			//{
			//    Project = p,
			//    TotalTasks = db.Task.Count(t => t.ProjectId == p.ProjectId),
			//    CompletedTasks = db.Task.Count(t => t.ProjectId == p.ProjectId && t.Status == "Completed")
			//})
			//.ToList();

			//    return View(projects);
			//
			var projects = db.AllProjects
			  .Include(p => p.Users)
			  .Select(p => new
			  {
				  p.ProjectId,
				  p.ProjectName,
				  p.Description,
				  p.ManagerName,
				  Members = p.Users.Select(u => u.ProfilePicture ?? "default.png").ToList(),
				  p.EndDate,
				  p.Priority,
				  p.Status,
				  TotalTasks = db.Task.Count(t => t.ProjectId == p.ProjectId),
				  CompletedTasks = db.Task.Count(t => t.ProjectId == p.ProjectId && t.Status == "Completed")
			  }).ToList();


			return View(projects);


		}




        public IActionResult ProjectUpload()
        {
   //         var users = db.User
			//.Select(u => new
			//{
			//	UserId = u.UserId,
			//	FullName = u.FirstName + " " + u.LastName
			//}).ToList();

            ViewBag.Users = db.User.ToList();


            ViewBag.Managers = db.User
			.Where(u => u.ReportingManager != null)
			.Select(u => u.ReportingManager)
			.Distinct()
			.ToList();

            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name))
            .DistinctBy(region => region.ISOCurrencySymbol) // Remove duplicates
            .OrderBy(region => region.CurrencyEnglishName) // Sort by currency name
            .Select(region => new
            {
                Value = region.ISOCurrencySymbol,
                Text = $"{region.CurrencyEnglishName} ({region.CurrencySymbol})"
            }).ToList();

            // Pass data to the View
            ViewBag.CurrencyList = new SelectList(cultures, "Value", "Text");


            return View();
        }



        [HttpPost]
		public IActionResult ProjectUpload(Projects model, IFormFile logo, IFormFile file, int[] Users)
		{
			string logoPath = null;
			string filePath = null;

			if (logo != null && logo.Length > 0)
			{
				string logoFileName = Guid.NewGuid() + Path.GetExtension(logo.FileName);
				string logoUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
				Directory.CreateDirectory(logoUploadPath);
				string fullLogoPath = Path.Combine(logoUploadPath, logoFileName);

				using (var stream = new FileStream(fullLogoPath, FileMode.Create))
				{
					logo.CopyTo(stream);
				}
				logoPath = "uploads/logos/" + logoFileName;
			}

			if (file != null && file.Length > 0)
			{
				string fileFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
				string fileUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "files");
				Directory.CreateDirectory(fileUploadPath);
				string fullFilePath = Path.Combine(fileUploadPath, fileFileName);

				using (var stream = new FileStream(fullFilePath, FileMode.Create))
				{
					file.CopyTo(stream);
				}
				filePath = "uploads/files/" + fileFileName; // ✅ Fixed Path
			}

			var project = new Projects
			{
				ProjectName = model.ProjectName,
				ClientName = model.ClientName,
				Description = model.Description,
				StartDate = model.StartDate,
				EndDate = model.EndDate,
				Priority = model.Priority,
				ProjectValue = model.ProjectValue,
				PriceType = model.PriceType,
				Status = model.Status,
				LogoPath = logoPath,
				FilePath = filePath,
				ManagerName = model.ManagerName,
				//Members = model.Members
			};
			if (Users != null)
			{
				var selectedUsers = db.User.Where(u => Users.Contains(u.UserId)).ToList();
				project.Users = selectedUsers; // Assuming you have a navigation property for Users


			}


			db.AllProjects.Add(project);
			db.SaveChanges();
			TempData["success"] = "Project Added Successfully!!!";
			return RedirectToAction("ProjectList");
		}

		public IActionResult ProjectList()
		{
			//var projects = db.AllProjects.ToList();

			var projects = db.AllProjects
			.Include(p => p.Users)
			.Select(p => new
			{
				p.ProjectId,
				p.ProjectName,
				p.ManagerName,
				Members = p.Users.Select(u => u.ProfilePicture ?? "default.png").ToList(),
				p.EndDate,
				p.Priority,
				p.Status
			}).ToList();


			return View(projects);

		}
		public IActionResult Delete(int id)
		{
			var project = db.AllProjects.Find(id);
			if (project != null)
			{
				db.AllProjects.Remove(project);
				db.SaveChanges();
			}
            TempData["error"] = "Project Deleted Successfully!!!";
            return RedirectToAction("ProjectList");
		}

		public IActionResult Edit(int id)
		{
         

            var project = db.AllProjects
                    .Include(p => p.Users) // Include team members
                    .FirstOrDefault(p => p.ProjectId == id);
            var users = db.User.Select(u => new
			{
				UserId = u.UserId,
				FullName = u.FirstName + " " + u.LastName  // Concatenating FirstName and LastName
			})
								  .ToList();
			if (project == null)
			{
				return NotFound();
			}


			ViewBag.Users = db.User.ToList();
			ViewBag.Departments = db.Departments.ToList();

            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
           .Select(culture => new RegionInfo(culture.Name))
           .DistinctBy(region => region.ISOCurrencySymbol) // Remove duplicates
           .OrderBy(region => region.CurrencyEnglishName) // Sort by currency name
           .Select(region => new
           {
               Value = region.ISOCurrencySymbol,
               Text = $"{region.CurrencyEnglishName} ({region.CurrencySymbol})"
           }).ToList();

            // Pass data to the View
            ViewBag.CurrencyList = new SelectList(cultures, "Value", "Text");


            // Status Dropdown: Ensure saved status is first
            string savedStatus = project.Status ?? "Inactive"; // Default to Inactive if null
            List<SelectListItem> statusList = new List<SelectListItem>
            {
                new SelectListItem { Value = savedStatus, Text = savedStatus, Selected = true },
                new SelectListItem { Value = savedStatus == "Active" ? "Inactive" : "Active", Text = savedStatus == "Active" ? "Inactive" : "Active" }
            };

            ViewBag.StatusList = statusList;

            return View(project);



		}

        //[HttpPost]
        //public IActionResult Edit(Projects model, IFormFile logo, IFormFile file, int[] Users)
        //{
        //	var existingProject = db.AllProjects.Find(model.ProjectId); // Find the existing project by ID
        //	if (existingProject == null)
        //	{
        //		return NotFound();
        //	}

        //	// Ensure PriceType is set to a default value if it's null or empty
        //	if (string.IsNullOrEmpty(model.PriceType))
        //	{
        //		model.PriceType = "Fixed"; // Default value, change as needed
        //	}

        //	string logoPath = existingProject.LogoPath; // Keep existing logo path
        //	string filePath = existingProject.FilePath; // Keep existing file path

        //	if (logo != null && logo.Length > 0)
        //	{
        //		// Delete the old logo if it exists
        //		if (!string.IsNullOrEmpty(existingProject.LogoPath))
        //		{
        //			var oldLogoPath = Path.Combine(Directory.GetCurrentDirectory(), existingProject.LogoPath);
        //			if (System.IO.File.Exists(oldLogoPath))
        //			{
        //				System.IO.File.Delete(oldLogoPath);
        //			}
        //		}

        //		// Save the new logo
        //		string logoFileName = Guid.NewGuid() + Path.GetExtension(logo.FileName);
        //		string logoUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
        //		Directory.CreateDirectory(logoUploadPath);
        //		string fullLogoPath = Path.Combine(logoUploadPath, logoFileName);

        //		using (var stream = new FileStream(fullLogoPath, FileMode.Create))
        //		{
        //			logo.CopyTo(stream);
        //		}
        //		logoPath = "wwwroot/uploads/logos/" + logoFileName;
        //	}

        //	if (file != null && file.Length > 0)
        //	{
        //		// Delete the old file if it exists
        //		if (!string.IsNullOrEmpty(existingProject.FilePath))
        //		{
        //			var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), existingProject.FilePath);
        //			if (System.IO.File.Exists(oldFilePath))
        //			{
        //				System.IO.File.Delete(oldFilePath);
        //			}
        //		}

        //		// Save the new file
        //		string fileFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        //		string fileUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "files");
        //		Directory.CreateDirectory(fileUploadPath);
        //		string fullFilePath = Path.Combine(fileUploadPath, fileFileName);

        //		using (var stream = new FileStream(fullFilePath, FileMode.Create))
        //		{
        //			file.CopyTo(stream);
        //		}
        //		filePath = "wwwroot/uploads/files/" + fileFileName;
        //	}

        //	// Update the properties of the existing entity
        //	existingProject.ProjectName = model.ProjectName;
        //	existingProject.ClientName = model.ClientName;
        //	existingProject.Description = model.Description;
        //	existingProject.StartDate = model.StartDate;
        //	existingProject.EndDate = model.EndDate;
        //	existingProject.Priority = model.Priority;
        //	existingProject.ProjectValue = model.ProjectValue;
        //	existingProject.PriceType = model.PriceType; // Ensure PriceType is updated
        //	existingProject.Status = model.Status;
        //	existingProject.LogoPath = logoPath;
        //	existingProject.FilePath = filePath;
        //	existingProject.ManagerName = model.ManagerName;
        //	//existingProject.Members = model.Members;
        //	if (Users != null)
        //	{
        //		var selectedUsers = db.User.Where(u => Users.Contains(u.UserId)).ToList();
        //		model.Users = selectedUsers; // Assuming you have a navigation property for Users
        //	}
        //	// Save changes to the database
        //	db.SaveChanges();
        //          TempData["upd"] = "Project Updated Successfully!!!";
        //          return RedirectToAction("ProjectList");
        //}

        [HttpPost]
        public IActionResult Edit(Projects model, IFormFile logo, IFormFile file, int[] Users)
        {
            var existingProject = db.AllProjects
                .Include(p => p.Users) // ✅ Ensure Users are included
                .FirstOrDefault(p => p.ProjectId == model.ProjectId); // Find the existing project

            if (existingProject == null)
            {
                return NotFound();
            }

            // Ensure PriceType is set to a default value if it's null or empty
            if (string.IsNullOrEmpty(model.PriceType))
            {
                model.PriceType = "Fixed"; // Default value, change as needed
            }

            string logoPath = existingProject.LogoPath; // Keep existing logo path
            string filePath = existingProject.FilePath; // Keep existing file path

            // ✅ Handle Logo Upload
            if (logo != null && logo.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingProject.LogoPath))
                {
                    var oldLogoPath = Path.Combine(Directory.GetCurrentDirectory(), existingProject.LogoPath);
                    if (System.IO.File.Exists(oldLogoPath))
                    {
                        System.IO.File.Delete(oldLogoPath);
                    }
                }

                string logoFileName = Guid.NewGuid() + Path.GetExtension(logo.FileName);
                string logoUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
                Directory.CreateDirectory(logoUploadPath);
                string fullLogoPath = Path.Combine(logoUploadPath, logoFileName);

                using (var stream = new FileStream(fullLogoPath, FileMode.Create))
                {
                    logo.CopyTo(stream);
                }
                logoPath = "wwwroot/uploads/logos/" + logoFileName;
            }

            // ✅ Handle File Upload
            if (file != null && file.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingProject.FilePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), existingProject.FilePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string fileFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string fileUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "files");
                Directory.CreateDirectory(fileUploadPath);
                string fullFilePath = Path.Combine(fileUploadPath, fileFileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                filePath = "wwwroot/uploads/files/" + fileFileName;
            }

            // ✅ Update Project Details
            existingProject.ProjectName = model.ProjectName;
            existingProject.ClientName = model.ClientName;
            existingProject.Description = model.Description;
            existingProject.StartDate = model.StartDate;
            existingProject.EndDate = model.EndDate;
            existingProject.Priority = model.Priority;
            existingProject.ProjectValue = model.ProjectValue;
            existingProject.PriceType = model.PriceType;
            existingProject.Status = model.Status;
            existingProject.LogoPath = logoPath;
            existingProject.FilePath = filePath;
            existingProject.ManagerName = model.ManagerName;

            // ✅ Fix: Update Team Members
            if (Users != null)
            {
                // Fetch the new selected users
                var selectedUsers = db.User.Where(u => Users.Contains(u.UserId)).ToList();

                // ✅ Ensure Users navigation property is loaded
                db.Entry(existingProject).Collection(p => p.Users).Load();

                // ✅ Remove all existing users associated with the project
                existingProject.Users.Clear();

                // ✅ Add new users manually
                foreach (var user in selectedUsers)
                {
                    existingProject.Users.Add(user);
                }
            }

            // ✅ Save Changes
            db.SaveChanges();
            TempData["upd"] = "Project Updated Successfully!!!";
            return RedirectToAction("ProjectList");
        }

    }
}