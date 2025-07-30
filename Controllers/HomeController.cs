using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;
using Pulse360.Models;
using System.Diagnostics;
using System.Linq;

namespace Pulse360.Controllers
{
    public class HomeController : Controller
    {
		private readonly ApplicationDbContext db;
		
		public HomeController(ApplicationDbContext db)
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
            

            // Handle file upload validation
            if (OrganizationLogoFile != null && OrganizationLogoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                var fileExtension = Path.GetExtension(OrganizationLogoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("OrganizationLogo", "Only PNG and JPG files are allowed.");
                    return View(org);
                }

                // Save the file
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/logos", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    OrganizationLogoFile.CopyTo(stream);
                }

                org.OrganizationLogo = $"/uploads/logos/{fileName}";
            }
            else
            {
                ModelState.AddModelError("OrganizationLogo", "Please upload a logo file.");
                return View(org);
            }

            db.Organization.Add(org);
            db.SaveChanges();
            TempData["success"] = "Organization added successfully!";
            return RedirectToAction("FetchOraganisation");
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
		public IActionResult EditOrganisation(Organization org,IFormFile OrganizationLogo)
		{
            Organization pr;
            if (OrganizationLogo == null)
            {
                pr = new Organization()
                {
                    OrganizationId = org.OrganizationId,
                    OrganizationName = org.OrganizationName,
                    OrganizationEmail = org.OrganizationEmail,
                    OrganizationLogo = db.Organization.Where(x => x.OrganizationId == org.OrganizationId).Select(x => x.OrganizationLogo).SingleOrDefault(),
                    OrganizationAddress = org.OrganizationAddress,
					OrganizationDescription=org.OrganizationDescription,
					OrganizationPhone=org.OrganizationPhone
                };
            }
            else
            {
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                var fileExtension = Path.GetExtension(OrganizationLogo.FileName).ToLower();

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
                    OrganizationLogo.CopyTo(stream);
                }

                // Store the file path in the database
                org.OrganizationLogo = $"/Uploads/logos/{fileName}";
                pr = new Organization()
                {
                    OrganizationId = org.OrganizationId,
                    OrganizationName = org.OrganizationName,
                    OrganizationEmail = org.OrganizationEmail,
                    OrganizationLogo = org.OrganizationLogo,
                    OrganizationAddress = org.OrganizationAddress,
                    OrganizationDescription = org.OrganizationDescription,
                    OrganizationPhone = org.OrganizationPhone
                };
            }

            db.Organization.Update(pr);
            db.SaveChanges();
            return RedirectToAction("FetchOraganisation");
        }

        

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


	}
}
