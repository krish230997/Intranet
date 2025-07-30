using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;

namespace Pulse360.Controllers
{
    public class AttendanceController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            var email = User.FindFirstValue(ClaimTypes.Name);


            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var user = _context.User.SingleOrDefault(x => x.Email == email);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var today = DateTime.Today;


            //var today = new DateTime(2025, 1, 27);


            ViewData["LoggedInUser"] = email;
            ViewBag.CurrentTime = DateTime.Now.ToString("hh:mm tt");

            var attendance = _context.Attendance.FirstOrDefault(a => a.Date == today && a.UserId == user.UserId);


            if (attendance == null && DateTime.Now.Hour >= 18)
            {
                attendance = new Attendance
                {
                    Date = today,
                    UserId = user.UserId,
                    Status = "Absent"
                };
                _context.Attendance.Add(attendance);
                _context.SaveChanges();

                SendAttendanceEmail(user.Email, "Absent");
            }

            ViewBag.AttendanceStatus = GetAttendanceStatus(attendance);




            if (attendance != null)
            {
                // Fetching data for the ViewBag
                ViewBag.WorkingHours = attendance.WorkingHours > 0 ? attendance.WorkingHours.ToString("0.#") : "0";
                ViewBag.BreakHours = attendance.BreakHours > 0 ? attendance.BreakHours.ToString("0.#") : "0";
                ViewBag.OvertimeHours = attendance.OvertimeHours > 0 ? attendance.OvertimeHours.ToString("0.#") : "0";
                ViewBag.ProductionHours = attendance.ProductionHours > 0 ? attendance.ProductionHours.ToString("0.#") : "0";
                ViewBag.LateMinutes = attendance.Late > 0 ? attendance.Late.ToString() : "0";
            }
            else
            {
                ViewBag.WorkingHours = "0";
                ViewBag.BreakHours = "0";
                ViewBag.OvertimeHours = "0";
                ViewBag.ProductionHours = "0";
                ViewBag.LateMinutes = "0";
            }

            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(6);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            decimal totalHoursToday = _context.Attendance
      .Where(a => a.Date == today && a.UserId == user.UserId)
      .Sum(a => a.WorkingHours);

            decimal totalHoursWeek = _context.Attendance
       .Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek && a.UserId == user.UserId)
       .Sum(a => a.WorkingHours);

            decimal totalHoursMonth = _context.Attendance
       .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
       .Sum(a => a.WorkingHours);

            decimal overtimeThisMonth = _context.Attendance
        .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
        .Sum(a => a.OvertimeHours);

            ViewBag.TotalWorkingHoursToday = totalHoursToday.ToString("0.#");
            ViewBag.TotalWorkingHoursWeek = totalHoursWeek.ToString("0.#");
            ViewBag.TotalWorkingHoursMonth = totalHoursMonth.ToString("0.#");
            ViewBag.OvertimeThisMonth = overtimeThisMonth.ToString("0.#");

            var userInfo = _context.User.FirstOrDefault(u => u.UserId == user.UserId);
            ViewBag.User = user;



            if (attendance != null)
            {
                var lunchIn = attendance.LunchIn;
                var lunchout = attendance.LunchOut;
                var checkin = attendance.CheckIn;
                var checkout = attendance.CheckOut;
                var standardCheckOut = new TimeSpan(18, 0, 0);
                var workinghrsinhoursandminutes = (checkout.HasValue && checkin.HasValue) ? checkout - checkin : (TimeSpan?)null;
                var breakhrsinhoursandminutes = (lunchout.HasValue && lunchIn.HasValue) ? lunchout - lunchIn : (TimeSpan?)null;
                var productionhrsinhoursandminutes = (workinghrsinhoursandminutes.HasValue && breakhrsinhoursandminutes.HasValue)
                    ? workinghrsinhoursandminutes - breakhrsinhoursandminutes
                    : (TimeSpan?)null;

                TimeSpan overtime = TimeSpan.Zero;

                if (checkout.HasValue && checkout.Value.TimeOfDay > standardCheckOut)
                {
                    overtime = checkout.Value.TimeOfDay - standardCheckOut;
                }

                string overtimeFormatted = $"{(int)overtime.TotalHours}h {overtime.Minutes}m";
                string workingFormatted = $"{(int)workinghrsinhoursandminutes.GetValueOrDefault().TotalHours}h {workinghrsinhoursandminutes.GetValueOrDefault().Minutes}m";
                string breakFormatted = $"{(int)breakhrsinhoursandminutes.GetValueOrDefault().TotalHours}h {breakhrsinhoursandminutes.GetValueOrDefault().Minutes}m";
                string productionFormatted = $"{(int)productionhrsinhoursandminutes.GetValueOrDefault().TotalHours}h {productionhrsinhoursandminutes.GetValueOrDefault().Minutes}m";

                ViewBag.OvertimeFormatted = overtimeFormatted;
                ViewBag.WorkingFormatted = workingFormatted;
                ViewBag.BreakFormatted = breakFormatted;
                ViewBag.ProductionFormatted = productionFormatted;
            }

            var attendanceList = _context.Attendance.Where(a => a.UserId == user.UserId).OrderByDescending(a => a.Date).ToList();
            ViewData["AttendanceList"] = attendanceList;

            return View(attendance ?? new Attendance { Date = today });

        }


        private string GetAttendanceStatus(Attendance attendance)
        {
            if (attendance == null || attendance.CheckIn == null)
            {
                return "Check-In";
            }
            else if (attendance.LunchIn == null)
            {
                return "Lunch-In";
            }
            else if (attendance.LunchOut == null)
            {
                return "Lunch-Out";
            }
            else if (attendance.CheckOut == null)
            {
                return "Check-Out";
            }
            else
            {
                return "Attendance Marked";
            }
        }


        [HttpPost]
        public IActionResult MarkAttendance()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);



            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = _context.User.Include(x => x.Role).SingleOrDefault(x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var today = DateTime.Today;
            //var today =new DateTime(2025, 1, 27);

            //var today = new DateTime(2025, 1, 27, 18, 40, 0);

            var attendance = _context.Attendance.FirstOrDefault(a => a.Date == today && a.UserId == user.UserId);

            if (attendance != null && attendance.Status == "Absent" && DateTime.Now.Hour >= 18)
            {
                TempData["ErrorMessage"] = "You cannot check in after 6 PM if marked absent.";
                return RedirectToAction("Index");
            }



            DateTime standardCheckIn = today.AddHours(9);  // 09:00 AM
            DateTime standardCheckOut = today.AddHours(18); // 06:00 PM

            if (attendance == null)
            {
                // First Check-In: Store actual time
                attendance = new Attendance
                {
                    Date = today,
                    CheckIn = DateTime.Now,
                    //CheckIn =new DateTime(2025, 1, 27, 09, 30, 0),
                    UserId = user.UserId,
                    Status = "Present"
                };
                _context.Attendance.Add(attendance);
            }
            else
            {
                if (!attendance.LunchIn.HasValue)
                {

                    attendance.LunchIn = DateTime.Now;
                    //attendance.LunchIn = new DateTime(2025, 1, 27, 13, 00, 0);
                }
                else if (!attendance.LunchOut.HasValue)
                {
                    attendance.LunchOut = DateTime.Now;
                    //attendance.LunchOut = new DateTime(2025, 1, 27, 14, 00, 0);
                }
                else if (!attendance.CheckOut.HasValue)
                {

                    attendance.CheckOut = DateTime.Now;
                    //attendance.CheckOut = new DateTime(2025, 1, 27, 18, 40, 0);
                    decimal workingHours = 0;
                    decimal breakHours = 0;
                    decimal overtimeHours = 0;
                    int lateMinutes = 0;

                    if (attendance.CheckIn.HasValue && attendance.CheckOut.HasValue)
                    {
                        TimeSpan totalWorkTime = attendance.CheckOut.Value - attendance.CheckIn.Value;

                        // Break Hours Calculation
                        if (attendance.LunchIn.HasValue && attendance.LunchOut.HasValue)
                        {
                            breakHours = (decimal)(attendance.LunchOut.Value - attendance.LunchIn.Value).TotalMinutes / 60;
                            breakHours = Math.Round(breakHours, 1);
                        }

                        workingHours = (decimal)totalWorkTime.TotalHours - breakHours;

                        //workingHours = Math.Round(workingHours, 1);
                    }

                    // Overtime Calculation
                    if (attendance.CheckOut.HasValue && attendance.CheckOut.Value.TimeOfDay > standardCheckOut.TimeOfDay)
                    {
                        overtimeHours = (decimal)(attendance.CheckOut.Value.TimeOfDay - standardCheckOut.TimeOfDay).TotalMinutes / 60;
                        overtimeHours = Math.Round(overtimeHours * 2, MidpointRounding.AwayFromZero) / 2;
                        //overtimeHours = Math.Round(overtimeHours, 1);
                    }

                    // Late Minutes Calculation
                    if (attendance.CheckIn.HasValue && attendance.CheckIn.Value.TimeOfDay > standardCheckIn.TimeOfDay)
                    {
                        lateMinutes = (int)(attendance.CheckIn.Value.TimeOfDay - standardCheckIn.TimeOfDay).TotalMinutes;

                        // Round late minutes to the nearest 30-minute interval
                        lateMinutes = (int)(Math.Ceiling(lateMinutes / 30.0) * 30);
                    }

                    // Production Hours Calculation
                    decimal productionHours = workingHours - breakHours;
                    productionHours = Math.Round(productionHours * 2, MidpointRounding.AwayFromZero) / 2;
                    //productionHours = Math.Round(productionHours, 1);



                    string status = workingHours <= 4.0m ? "Half Day" : "Present";


                    attendance.WorkingHours = workingHours;
                    attendance.BreakHours = breakHours;
                    attendance.OvertimeHours = overtimeHours;
                    attendance.Late = lateMinutes;
                    attendance.ProductionHours = productionHours;
                    attendance.Status = status;



                    _context.SaveChanges();
                    _context.Entry(attendance).Reload();

                    if (status == "Half Day")
                    {
                        SendAttendanceEmail(user.Email, "Half Day");
                    }

                    //_context.Entry(attendance).State = EntityState.Modified;
                }
            }

            //if (attendance.CheckIn == null && attendance.LunchIn == null && attendance.LunchOut == null && attendance.CheckOut == null)
            //{
            //    attendance.Status = "Absent";
            //}

            try
            {
                _context.SaveChanges();
                Console.WriteLine("Changes saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
            }


            ViewBag.CheckInTime = attendance.CheckIn?.ToString("hh:mm tt");
            ViewBag.WorkingHours = attendance.WorkingHours;
            ViewBag.BreakHours = attendance.BreakHours;
            ViewBag.OvertimeHours = attendance.OvertimeHours;
            ViewBag.LateMinutes = attendance.Late;
            ViewBag.ProductionHours = attendance.ProductionHours;

            return RedirectToAction("Index");

        }




        public IActionResult FilteredAttendanceAction(string searchString, string status, DateTime? startDate, DateTime? endDate, int rowsPerPage = 10, int page = 1, string sortBy = "recent")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.User.SingleOrDefault(x => x.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var query = _context.Attendance.Where(x => x.UserId == user.UserId).AsQueryable();


            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                query = query.Where(x =>
                    (x.Status != null && x.Status.ToLower().Contains(searchString)) ||
                    (x.Date != null && x.Date.ToString("yyyy-MM-dd").Contains(searchString)) ||
                    (x.CheckIn.HasValue && x.CheckIn.Value.ToString("hh:mm tt").ToLower().Contains(searchString)) ||
                    (x.CheckOut.HasValue && x.CheckOut.Value.ToString("hh:mm tt").ToLower().Contains(searchString)) ||
                    (x.BreakHours.ToString() ?? "").Contains(searchString) ||
                    (x.Late.ToString() ?? "").Contains(searchString) ||
                    (x.OvertimeHours.ToString() ?? "").Contains(searchString) ||
                    (x.ProductionHours.ToString() ?? "").Contains(searchString));
            }



            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(x => x.Status.ToLower() == status.ToLower());
            }

            if (startDate.HasValue)
            {
                query = query.Where(x => x.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.Date.Date <= endDate.Value.Date);
            }

            switch (sortBy)
            {
                case "asc":
                    query = query.OrderBy(x => x.Date);
                    break;
                case "desc":
                    query = query.OrderByDescending(x => x.Date); // Adjust column as needed
                    break;
                case "recent":
                    query = query.OrderByDescending(x => x.Date); // Most recent first
                    break;
                case "last7Days":
                    query = query.Where(x => x.Date >= DateTime.Now.AddDays(-7)).OrderByDescending(x => x.Date);
                    break;
                case "lastMonth":
                    query = query.Where(x => x.Date >= DateTime.Now.AddMonths(-1)).OrderByDescending(x => x.Date);
                    break;
                default:
                    query = query.OrderByDescending(x => x.Date); // Default sorting
                    break;
            }

            int totalRecords = query.Count();
            var attendanceList = query.Skip((page - 1) * rowsPerPage).Take(rowsPerPage).ToList();

            ViewData["SearchString"] = searchString;
            ViewData["Status"] = status;
            ViewData["RowsPerPage"] = rowsPerPage;
            ViewData["TotalRecords"] = totalRecords;
            ViewData["CurrentPage"] = page;
            ViewData["SortBy"] = sortBy;
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;



            return Json(attendanceList);
        }





        public void SendAttendanceEmail(string userEmail, string attendanceStatus)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                string todayDate = currentDate.ToString("dd-MMM-yyyy");

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("sakshimsawant162@gmail.com");
                mail.To.Add(userEmail);
                mail.Subject = $"Attendance Notification - {attendanceStatus} on {todayDate}";
                mail.Body = $"Dear {userEmail},\n\nYou have been marked as **{attendanceStatus}** on {todayDate}. If this is incorrect, please contact HR.\n\nRegards,\nCompany HR";

                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential("sakshimsawant162@gmail.com", "czscembogqficlkq");
                smtp.Send(mail);

                Console.WriteLine($"✅ Attendance email sent to {userEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending attendance email: {ex.Message}");
            }
        }


    }
}

