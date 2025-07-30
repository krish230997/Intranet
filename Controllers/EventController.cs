using Pulse360.Data;
using Pulse360.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Pulse360.Controllers
{
    public class EventController : Controller
    {

        private readonly ApplicationDbContext _db;

        public EventController(ApplicationDbContext db)
        {

            _db = db;
        }

        public IActionResult Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);

            ViewBag.UserRole = role; // Pass the role to the view
            return View();
        }
        public IActionResult Index2()
        {
            var eventTypes = _db.Events.ToList();
            return View(eventTypes);
        }



        [HttpGet]
        public IActionResult GetEventTypes()
        {
            var eventTypes = _db.EventTypes.Select(et => new
            {
                id = et.Id,
                name = et.Name,
                color = et.Color
            }).ToList();
            return Json(eventTypes);
        }

        [HttpGet]
        public IActionResult GetEvents()
        {
            var events = new List<object>();

            // Fetch active events from the database
            var dbEvents = _db.Events
                .Join(_db.EventTypes,
                    e => e.EventTypeId,
                    et => et.Id,
                    (e, et) => new
                    {
                        title = e.Title,
                        start = e.Date,
                        color = et.Color,
                        status = e.Status // Include status to filter
                    })
                .Where(e => e.status == "Active") // Only include active events
                .ToList();

            events.AddRange(dbEvents);

            return Json(events);
        }

        [HttpPost]
        public IActionResult AddEvent([FromBody] EventModel newEvent)
        {


            newEvent.Status = "Active";

            _db.Events.Add(newEvent);
            _db.SaveChanges();

            // Create a notification message


            return Json(new { success = true });


            return BadRequest(new { success = false });
        }

        public IActionResult evtype()
        {
            var eventTypes = _db.EventTypes.ToList();
            return View(eventTypes);
        }

        [HttpPost]
        public IActionResult Create([FromBody] EventTypes eventTypess)
        {
            if (ModelState.IsValid)
            {
                _db.EventTypes.Add(eventTypess);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            return BadRequest(new { success = false });
        }
        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var eventTypess = _db.EventTypes.Find(id);
            if (eventTypess != null)
            {
                _db.EventTypes.Remove(eventTypess);
                _db.SaveChanges();
            }
            return RedirectToAction("evtype");
        }
        //public IActionResult EditEvent(int id)
        //{
        //    var eventTypess = _db.EventTypes.Find(id);
        //    if (eventTypess == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(eventTypess);
        //}

        [HttpPost]
        public IActionResult EditEvent([FromBody] EventModel eventTypess)
        {
            //if (eventTypess == null || eventTypess.Id == 0)
            //{
            //    return BadRequest(new { message = "Invalid event data." });
            //}

            var existingEvent = _db.Events.Find(eventTypess.Id);
            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            // Update event properties
            existingEvent.Title = eventTypess.Title;
            existingEvent.Date = eventTypess.Date;
            existingEvent.EventTypeId = eventTypess.EventTypeId;

            //_db.Events.Update(existingEvent);
            _db.SaveChanges(); // Synchronous save operation

            return Ok(new { message = "Event updated successfully!" });
        }

        [HttpPost]
        public IActionResult DeleteSubEvent(int id)
        {
            // Assuming you have a method to delete the event by ID
            var eventToDelete = _db.Events.Find(id);
            if (eventToDelete != null)
            {
                _db.Events.Remove(eventToDelete);
                _db.SaveChanges();
            }
            return RedirectToAction("Index2");
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var eventItem = _db.Events.Find(id);
            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            // Toggle status
            eventItem.Status = eventItem.Status == "Active" ? "Inactive" : "Active";

            _db.SaveChanges();

            return Ok(new { status = eventItem.Status });
        }

    }
}