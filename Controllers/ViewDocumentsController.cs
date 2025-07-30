using Microsoft.AspNetCore.Mvc;
using Pulse360.Data;

namespace Pulse360.Controllers
{
    public class ViewDocumentsController : Controller
    {
        private readonly ApplicationDbContext db;

        public ViewDocumentsController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            //var email = HttpContext.Session.GetString("em");
            var email = User.Identity.Name;
            var data = db.AdminDocuments.Where(x => x.Email.Equals(email)).ToList();
            return View(data);
        }

        public IActionResult IndexAtAdmin()
        {
            //var email = HttpContext.Session.GetString("em");
            var email = User.Identity.Name;
            var data = db.AdminDocuments.ToList();
            return View(data);
        }

        public IActionResult ViewPdf(int id)
        {
            var data = db.AdminDocuments.FirstOrDefault(x => x.AdminDocId.Equals(id));

            if (data == null || string.IsNullOrEmpty(data.DocFile))
                return NotFound();
            if (data.DocName.Equals("Offer-letter"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "offer-letters", data.DocFile);
                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                return File(System.IO.File.OpenRead(filePath), "application/pdf");
            }
            if (data.DocName.Equals("Resignation-letter"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "resignation-letters", data.DocFile);
                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                return File(System.IO.File.OpenRead(filePath), "application/pdf");
            }

            return View();

        }

        public IActionResult DownloadPDF(int id)
        {
            var data = db.AdminDocuments.FirstOrDefault(x => x.AdminDocId.Equals(id));
            if (data == null || string.IsNullOrEmpty(data.DocFile))
                return NotFound();

            if (data.DocName.Equals("Offer-letter"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "offer-letters", data.DocFile);

                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                return File(System.IO.File.OpenRead(filePath), "application/pdf", Path.GetFileName(filePath));
            }

            if (data.DocName.Equals("Resignation-letter"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "resignation-letters", data.DocFile);
                if (!System.IO.File.Exists(filePath))
                    return NotFound();

                return File(System.IO.File.OpenRead(filePath), "application/pdf", Path.GetFileName(filePath));
            }

            return View();
        }
    }
}
