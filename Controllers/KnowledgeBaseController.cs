using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Pulse360.Controllers
{
    public class KnowledgeBaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KnowledgeBaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Display Topics
        public IActionResult Index()
        {
            // Retrieve the user's role from claims
            var role = User.FindFirstValue(ClaimTypes.Role);

            var topics = _context.KnowledgeBaseTopics
                .Include(t => t.SubTopics)
                .ToList();

            ViewBag.UserRole = role; // Pass the role to the view
            return View(topics);
        }

        // AJAX-based Search Action that returns HTML
        public IActionResult Search(string searchQuery)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);

            var topicsQuery = _context.KnowledgeBaseTopics
                .Include(t => t.SubTopics)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Make the search case-insensitive
                topicsQuery = topicsQuery.Where(t => t.MasterTopic.ToLower().Contains(searchQuery.ToLower()));
            }

            var topics = topicsQuery.ToList();

            ViewBag.UserRole = role;

            StringBuilder htmlContent = new StringBuilder();

            if (topics != null && topics.Any())
            {
                foreach (var topic in topics)
                {
                    htmlContent.Append("<div class='col-md-4 mb-3'>");
                    htmlContent.Append("<div class='card'>");
                    htmlContent.Append("<div class='card-header bg-primary text-white'>");
                    htmlContent.Append("<h5>" + topic.MasterTopic + "</h5>");
                    htmlContent.Append("</div>");
                    htmlContent.Append("<div class='card-body'>");

                    if (topic.SubTopics != null && topic.SubTopics.Any())
                    {
                        htmlContent.Append("<ul class='list-group'>");
                        foreach (var subTopic in topic.SubTopics)
                        {
                            htmlContent.Append("<li class='list-group-item'>" + subTopic.Title + "</li>");
                        }
                        htmlContent.Append("</ul>");
                    }
                    else
                    {
                        htmlContent.Append("<p>No sub-topics available.</p>");
                    }

                    htmlContent.Append("</div></div></div>");
                }
            }
            else
            {
                htmlContent.Append("<p>No topics available. Click 'Add New Topic' to create one.</p>");
            }

            return Content(htmlContent.ToString(), "text/html");
        }

        // GET: Add Topic Form
        [Authorize(Roles = "Admin")]
        public IActionResult AddTopic()
        {
            return View();
        }

        // POST: Save Topic to Database
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult AddTopic(string MasterTopic, List<string> subTopics)
        {
            if (string.IsNullOrEmpty(MasterTopic))
            {
                ModelState.AddModelError("MasterTopic", "Master topic is required.");
            }

            if (subTopics == null || !subTopics.Any())
            {
                ModelState.AddModelError("SubTopics", "At least one sub-topic is required.");
            }

            if (ModelState.IsValid)
            {
                var topic = new KnowledgeBaseTopic
                {
                    MasterTopic = MasterTopic,
                    SubTopics = subTopics.Select(sub => new SubTopic { Title = sub }).ToList()
                };

                _context.KnowledgeBaseTopics.Add(topic);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View();
        }
    }
}