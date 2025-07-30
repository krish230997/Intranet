using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.Security.Claims;

namespace Pulse360.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public FileUploadController(IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //var email = HttpContext.Session.GetString("em");
            var email = User.Identity.Name;
            ViewBag.Users = _context.User.Where(a => a.Email.Equals(email)).Select(u => new
            {
                u.UserId,
                u.Email
            }).ToList();
            ViewBag.EmployeeDocumentType = new SelectList(_context.addEmployeeDocNames.ToList(), "DocName", "DocName");
            return View();
        }

        public async Task<IActionResult> SaveFiles(FileUploadViewModel model)
        {
            var email = User.Identity.Name;
            ViewBag.Users = _context.User.Where(a => a.Email.Equals(email)).Select(u => new
            {
                u.UserId,
                u.Email
            }).ToList();
            ViewBag.EmployeeDocumentType = new SelectList(_context.addEmployeeDocNames.ToList(), "DocName", "DocName");
            if (model.Files == null || model.Files.Count == 0)
            {
                ModelState.AddModelError("", "Please upload at least one file.");
                return View("Index", model);
            }

            if (model.Files.Count != model.DocumentTypes.Count)
            {
                ModelState.AddModelError("", "Number of files and document types must match.");
                return View("Index", model);
            }

            var user = _context.User.Find(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("Index", model);
            }
            int i;
            for (i = 0; i < model.Files.Count; i++)
            {
                var file = model.Files[i];
                var documentType = model.DocumentTypes[i];

                if (file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    // Define restricted extensions
                    var restrictedExtensions = new List<string> { ".doc", ".docx", ".xlsx", ".csv" };

                    if (restrictedExtensions.Contains(fileExtension))
                    {
                        TempData["error"] = $"Files with extension {fileExtension} are not allowed!";
                        return View("Index", model);
                    }
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Store the relative path instead of full path
                    var relativePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");

                    var fileUpload = new FileUpload
                    {
                        FileName = documentType,
                        FilePath = relativePath, // Save relative path in the model
                        UserId = model.UserId,
                        User = user
                    };

                    _context.FileUploads.Add(fileUpload);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Files uploaded successfully!";
            return RedirectToAction("Index");
        }


        public IActionResult FetchData()
        {
            ViewBag.Users = _context.User.Where(a => a.Role.Equals("Employee")).Select(u => new
            {
                u.UserId,
                u.Email
            }).ToList();
            return View();
        }

        public IActionResult GetUserDocuments(int userId)
        {
            var user = _context.User.Include(u => u.FileUploads)
                                      .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("Index");
            }

            var documents = user.FileUploads;

            return PartialView("_UserDocuments", documents); // Return the documents in a partial view
        }




        [HttpGet]
        public IActionResult DownloadDocument(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();  // Return 404 if file path is empty or invalid
            }

            // Remove the 'uploads/' part from the filePath to avoid double 'uploads' in the path
            filePath = filePath.Replace("uploads/", "");

            // Combine the web root path and file path to get the full file path
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            string fullPath = Path.Combine(uploadsFolder, filePath);


            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            // Return the file for download directly from the wwwroot/uploads folder
            return PhysicalFile(fullPath, "application/pdf", Path.GetFileName(fullPath));
        }

        public IActionResult AdminFileUpload()
        {
            ViewBag.Users = _context.User.Where(a => a.Role.RoleName != "Admin").Select(u => new
            {
                u.UserId,
                u.Email
            }).ToList();
            ViewBag.AdminDocumentType = new SelectList(_context.addAdminDocNames.ToList(), "DocName", "DocName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminFileUpload(FileUploadViewModel model)
        {
            ViewBag.Users = _context.User.Where(a => a.Role.RoleName != "Admin").Select(u => new
            {
                u.UserId,
                u.Email
            }).ToList();
            ViewBag.AdminDocumentType = new SelectList(_context.addAdminDocNames.ToList(), "DocName", "DocName");
            if (model.Files == null || model.Files.Count == 0)
            {
                ModelState.AddModelError("", "Please upload at least one file.");
                return View("AdminFileUpload", model);
            }

            if (model.Files.Count != model.DocumentTypes.Count)
            {
                ModelState.AddModelError("", "Number of files and document types must match.");
                return View("AdminFileUpload", model);
            }

            var user = _context.User.Find(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("AdminFileUpload", model);
            }

            for (int i = 0; i < model.Files.Count; i++)
            {
                var file = model.Files[i];
                var documentType = model.DocumentTypes[i];

                if (file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    // Define restricted extensions
                    var restrictedExtensions = new List<string> { ".doc", ".docx", ".xlsx", ".csv" };

                    if (restrictedExtensions.Contains(fileExtension))
                    {
                        TempData["error"] = $"Files with extension {fileExtension} are not allowed!";
                        return View("AdminFileUpload", model);
                    }
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Store the relative path instead of full path
                    var relativePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");

                    var fileUpload = new FileUpload
                    {
                        FileName = documentType,
                        FilePath = relativePath, // Save relative path in the model
                        UserId = model.UserId,
                        User = user
                    };

                    _context.FileUploads.Add(fileUpload);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Files uploaded successfully!";
            return RedirectToAction("AdminFileUpload");
        }

        //fetch emp docs wich added by admin
        public IActionResult fetchempdoc()
        {
            //var userid = int.Parse(HttpContext.Session.GetString("id"));
            var userid = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            var roleid = _context.User.Where(x => x.UserId.Equals(userid)).Select(x => x.RoleId).FirstOrDefault();
            var role = _context.Role.Where(x => x.RoleId.Equals(roleid)).Select(x => x.RoleName).FirstOrDefault();
            if (role.Equals("Admin"))
            {
                var data = _context.FileUploads.ToList();
                return View(data);
            }

            if (role.Equals("Employee"))
            {
                var data = _context.FileUploads.Where(x => x.UserId.Equals(userid)).ToList();
                //ViewBag.Users = _context.FileUploads.Where(a => a.UserId.Equals(userid)).Select(u => new
                //{
                //	u.UserId
                //}).ToList();
                return View(data);
            }
            return View();
        }

        public IActionResult ViewPdf(int id)
        {
            var data = _context.FileUploads.FirstOrDefault(x => x.id.Equals(id));

            if (data == null || string.IsNullOrEmpty(data.FilePath))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", data.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var fileExtension = Path.GetExtension(data.FilePath).ToLower();
            if (fileExtension == ".jpeg" || fileExtension == ".jpg" || fileExtension == ".png")
            {
                return File(System.IO.File.OpenRead(filePath), "image/" + fileExtension.Substring(1));
            }

            // For PDF files
            if (fileExtension == ".pdf")
            {
                return File(System.IO.File.OpenRead(filePath), "application/pdf");
            }

            // For Word files (.docx)
            if (fileExtension == ".docx")
            {
                string fileUrl = $"{Request.Scheme}://{Request.Host}/Uploads/{data.FilePath}";

                // Encode URL to ensure special characters don't break it
                string encodedUrl = Uri.EscapeDataString(fileUrl);

                // Google Docs Viewer Link
                string googleDocsUrl = $"https://docs.google.com/gview?url={encodedUrl}&embedded=true";

                return Redirect(googleDocsUrl);
            }


            return View();


        }

        public IActionResult DownloadPDF(int id)
        {
            var data = _context.FileUploads.FirstOrDefault(x => x.id.Equals(id));
            if (data == null || string.IsNullOrEmpty(data.FilePath))
                return NotFound();


            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", data.FilePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            return File(System.IO.File.OpenRead(filePath), "application/pdf", Path.GetFileName(filePath));

        }

        public IActionResult RemovePDF(int id)
        {
            var data = _context.FileUploads.Find(id);

            if (data != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", data.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                _context.FileUploads.Remove(data);
                _context.SaveChanges();
            }
            var updatedFileList = _context.FileUploads.ToList(); ;
            return View("fetchempdoc", updatedFileList);
        }
    }
}
