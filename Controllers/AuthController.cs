using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pulse360.Models;
using System.Security.Claims;
using Pulse360.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using System.Globalization;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Pulse360.Controllers
{
	public class AuthController : Controller
	{
		private readonly ApplicationDbContext db;
		public AuthController(ApplicationDbContext db)
		{
			this.db = db;
		}
		public IActionResult AdminRegistration()
		{
			// Prepare dropdown data
			ViewBag.Roles = new SelectList(db.Role.ToList(), "RoleId", "RoleName");
			ViewBag.Departments = new SelectList(db.Departments.ToList(), "DepartmentId", "Name");
			ViewBag.Designations = new SelectList(db.Designations.ToList(), "DesignationId", "Name");

			return View();
		}

		[HttpPost]
		public IActionResult AdminRegistration(User user, IFormFile profilePhoto)
		{
			if (profilePhoto != null && profilePhoto.Length > 0)
			{
				// Validate file type
				var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
				var fileExtension = Path.GetExtension(profilePhoto.FileName).ToLower();

				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("OrganizationLogo", "Only PNG and JPG files are allowed.");
					return View(user);
				}

				// Save the file
				var fileName = $"{profilePhoto.FileName}";
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Content", "ProfilePhoto", fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					profilePhoto.CopyTo(stream);
				}

				// Store the file path in the database
				user.ProfilePicture = $"/Content/ProfilePhoto/{fileName}";
			}

			user.DepartmentId = 1;
			user.DesignationtId = 1;
			db.User.Add(user); // Add user to the database
			db.SaveChanges();  // Save changes
			return RedirectToAction("SignIn"); // Redirect to the list of users


		}


		public IActionResult SignIn()
		{
			var data = db.Organization.OrderByDescending(x=>x.OrganizationId).Take(1).SingleOrDefault();
			if(data!=null)
			{
                ViewBag.logo = data.OrganizationLogo;
                ViewBag.name = data.OrganizationName;
                return View();
            }
			else
			{
                return View();
            }
			
		}

		[HttpPost]
		public async Task<IActionResult> SignIn(string email, string password)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				ViewBag.Error = "Email and Password are required.";
				return View();
			}

			// Retrieve the user from the database
			var emaildata = db.User
				.Include(u => u.Role)
				.SingleOrDefault(x => x.Email.Equals(email));
			if (emaildata != null)
			{
				// Check if the password matches
				var pass = emaildata.PasswordHash.Equals(password);  // Use the hashed password here
				if (pass)
				{
					if(emaildata.Status.Equals("Active"))
					{
						// Create claims
						var identity = new ClaimsIdentity(new[]
						{
					new Claim(ClaimTypes.Name, emaildata.Email),
					new Claim(ClaimTypes.Role, emaildata.Role.RoleName),
					new Claim(ClaimTypes.NameIdentifier, emaildata.UserId.ToString())// Assuming you have Role navigation
				}, CookieAuthenticationDefaults.AuthenticationScheme);


						
						// Create principal
						var principal = new ClaimsPrincipal(identity);

						// Sign in the user
						await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

						// Set session data
						//HttpContext.Session.SetString("User", emaildata.Email);
						HttpContext.Session.SetString("User", emaildata.Email);
						HttpContext.Session.SetString("UserN", emaildata.FirstName);
						HttpContext.Session.SetString("Name", $"{emaildata.FirstName} {emaildata.LastName}");
						// Redirect based on role
						HttpContext.Session.SetString("Email", emaildata.Email);
						// HttpContext.Session.SetString("Role", emaildata.Role);
						HttpContext.Session.SetString("Epath", emaildata.ProfilePicture);
						if (emaildata.Role.RoleName.Equals("Admin"))
						{
							return RedirectToAction("AdminD");
						}
						if (emaildata.Role.RoleName.Equals("Employee"))
						{
							return RedirectToAction("EmpD");
						}
						if (emaildata.Role.RoleName.Equals("Manager"))
						{
							return RedirectToAction("ManagerD");
						}
						if (emaildata.Role.RoleName.Equals("Super Admin"))
						{
							return RedirectToAction("SuperAdminD", "Dashboard");
						}
						if (emaildata.Role.RoleName.Equals("HR"))
						{
							return RedirectToAction("HRD", "HRD");
						}
					}
					else
					{
						TempData["error"] = "Your Account is Temporarily Blocked!!";
						
					}
					
				}

				else
				{
					TempData["error"] = "Invalid Password!!";

                    

                }

			}
			
			else
			{
				TempData["error"] = "Invalid Email!!";
				
			}

            var data = db.Organization.OrderByDescending(x => x.OrganizationId).Take(1).SingleOrDefault();
            if (data != null)
            {
                ViewBag.logo = data.OrganizationLogo;
                ViewBag.name = data.OrganizationName;
                return View();
            }
			else
			{
				return View();
			}
        }
		//admin db
		[Authorize(Roles = "Admin")]
		public IActionResult AdminD()
		{
            //Attendance 
            int totalEmployees = db.User.Count();
            int presentCount = db.Attendance.Count(a => a.Status == "Present");
            ViewBag.PresentRatio = $"{presentCount}/{totalEmployees}";

            //Project 
            int totalProjects = db.AllProjects.Count();
            int activeProjects = db.AllProjects.Count(p => p.Status == "Active");
            ViewBag.ProjectStatusRatio = $"{activeProjects}/{totalProjects}";


            //Client
            int totalClients = db.AllProjects
                                 .Select(p => p.ClientName)
                                 .Distinct()
                                 .Count();
            int activeClients = db.AllProjects
                                  .Where(p => p.Status == "Active")
                                  .Select(p => p.ClientName)
                                  .Distinct()
                                  .Count();
            ViewBag.ClientStatusRatio = $"{activeClients}/{totalClients}";


            ViewBag.TotalClients = totalClients;

            //Task
            int totalTasks = db.Task.Count();
            ViewBag.TotalTasks = totalTasks;

            //Earnings
            var totalEarnings = db.AllProjects.Sum(p => p.ProjectValue);
            var formattedTotalEarnings = totalEarnings.ToString("C0", CultureInfo.GetCultureInfo("en-US"));
            ViewBag.TotalEarnings = formattedTotalEarnings;

			//Fetching Employees with the department
			ViewBag.Users = db.User
			   .Include(u => u.Department)
			   .Select(u => new
			   {
				   ProfilePicture = u.ProfilePicture ?? "/assets/img/default-avatar.jpg",
				   FullName = u.FirstName + " " + u.LastName,
				   DepartmentName = u.Department != null ? u.Department.Name : "Not Assigned"
			   })
			   .Take(6)
			   .ToList();

			// Fetch Users with Birthday Today
			var today = DateTime.Today;
			var tomorrow = today.AddDays(1);

			var birthdayUsers = db.User
				.Where(u => u.DateOfBirth.Month == today.Month && u.DateOfBirth.Day == today.Day)
				.Select(u => new
				{
					FullName = u.FirstName + " " + u.LastName,
					ProfilePicture = !string.IsNullOrEmpty(u.ProfilePicture) ? u.ProfilePicture : "/assets/img/default-avatar.jpg",
					Designation = u.Designation != null ? u.Designation.Name : "N/A"
				})
				.ToList();

			var tomorrowBirthdayUsers = db.User
				.Where(u => u.DateOfBirth.Month == tomorrow.Month && u.DateOfBirth.Day == tomorrow.Day)
				.Select(u => new
				{
					FullName = u.FirstName + " " + u.LastName,
					ProfilePicture = !string.IsNullOrEmpty(u.ProfilePicture) ? u.ProfilePicture : "/assets/img/default-avatar.jpg",
					Designation = u.Designation != null ? u.Designation.Name : "N/A"
				})
				.ToList();

			ViewBag.TodayBirthdays = birthdayUsers;
			ViewBag.TomorrowBirthdays = tomorrowBirthdayUsers;

			var projects = db.AllProjects
		.Include(p => p.Users)
		.OrderByDescending(p => p.EndDate)
		.Take(8)
		.Select(p => new
		{
			p.ProjectId,
			p.ProjectName,
			Members = p.Users.Select(u => u.ProfilePicture ?? "default.png").ToList(),
			p.EndDate,
			p.Priority,
			p.Status
		}).ToList();

			int totalTaskss = db.Task.Count();
			int completedTasks = db.Task.Count(t => t.Status == "Completed");


			var taskStatusCounts = db.Task
				.GroupBy(t => t.Status)
				.Select(g => new
				{
					Status = g.Key,
					Count = g.Count()
				}).ToList();

			var taskStats = taskStatusCounts.ToDictionary(t => t.Status, t => t.Count);

			int onHold = taskStats.ContainsKey("Onhold") ? taskStats["Onhold"] : 0;
			int inProgress = taskStats.ContainsKey("Inprogress") ? taskStats["Inprogress"] : 0;
			int pending = taskStats.ContainsKey("Pending") ? taskStats["Pending"] : 0;


			var viewModel = new
			{
				Projects = projects,
				TaskStatistics = new
				{
					TotalTasks = totalTaskss,
					CompletedTasks = completedTasks,
					CompletedPercentage = totalTaskss > 0 ? (completedTasks * 100 / totalTaskss) : 0,
					OnHold = onHold,
					OnHoldPercentage = totalTaskss > 0 ? (onHold * 100 / totalTaskss) : 0,
					InProgress = inProgress,
					InProgressPercentage = totalTaskss > 0 ? (inProgress * 100 / totalTaskss) : 0,
					Pending = pending,
					PendingPercentage = totalTaskss > 0 ? (pending * 100 / totalTaskss) : 0
				}

			};

			//Employee Total Count
			var empCount = db.User.ToList();
			int totalCount = empCount.Count();
			ViewBag.TotalCounts = totalCount;

			//Attendance OverView Analytics
			var attendanceData = db.Attendance.ToList();
			int totalRecords = attendanceData.Count();

			int presentCounts = attendanceData.Count(a => a.Status == "Present");
			int absentCount = attendanceData.Count(a => a.Status == "Absent");
			int lateCount = attendanceData.Count(a => a.Status == "Late");
			int halfDayCount = attendanceData.Count(a => a.Status == "Half Day");

			double totalWorkingHours = (double)attendanceData.Sum(a => a.WorkingHours);
			double totalBreakHours = (double)attendanceData.Sum(a => a.BreakHours);
			double totalOvertimeHours = (double)attendanceData.Sum(a => a.OvertimeHours);
			double totalProductionHours = (double)attendanceData.Sum(a => a.ProductionHours);

			// Pass calculated hours to ViewBag
			ViewBag.TotalWorkingHours = totalWorkingHours;
			ViewBag.TotalBreakHours = totalBreakHours;
			ViewBag.TotalOvertimeHours = totalOvertimeHours;
			ViewBag.TotalProductionHours = totalProductionHours;

			ViewBag.PresentCount = presentCounts;
			ViewBag.AbsentCount = absentCount;
			ViewBag.LateCount = lateCount;
			ViewBag.HalfDayCount = halfDayCount;
			ViewBag.TotalCount = totalRecords;

			// Calculate percentage (avoid division by zero)
			ViewBag.PresentPercentage = totalRecords > 0 ? (presentCounts * 100) / totalRecords : 0;
			ViewBag.AbsentPercentage = totalRecords > 0 ? (absentCount * 100) / totalRecords : 0;
			ViewBag.LatePercentage = totalRecords > 0 ? (lateCount * 100) / totalRecords : 0;
			ViewBag.HalfDayPercentage = totalRecords > 0 ? (halfDayCount * 100) / totalRecords : 0;

			/// Calculate percentage (avoid division by zero) and round to 2 decimal places
			ViewBag.TotalWorkingHoursp = totalRecords > 0 ? Math.Round((totalWorkingHours * 100) / totalRecords, 2) : 0;
			ViewBag.TotalBreakHoursp = totalRecords > 0 ? Math.Round((totalBreakHours * 100) / totalRecords, 2) : 0;
			ViewBag.TotalOvertimeHoursp = totalRecords > 0 ? Math.Round((totalOvertimeHours * 100) / totalRecords, 2) : 0;
			ViewBag.TotalProductionHoursp = totalRecords > 0 ? Math.Round((totalProductionHours * 100) / totalRecords, 2) : 0;

			return View(viewModel);			


		}


		//emp db
		[Authorize(Roles = "Employee")]
		//public IActionResult EmpD()
		//{
		//    string userId = User.Identity.Name;
		//    if (string.IsNullOrEmpty(userId))
		//    {
		//        return RedirectToAction("SignIn", "Auth");
		//    }

		//    // Fetch the TotalLeaves for the logged-in user
		//    var leaveBalance = db.LeaveBalances
		//        .Where(lb => lb.User.Email == userId)
		//        .Select(lb => new { lb.TotalLeaves, lb.UsedLeaves })
		//        .SingleOrDefault();

		//    ViewBag.TotalLeaves = leaveBalance?.TotalLeaves ?? 0;
		//    ViewBag.UsedLeaves = leaveBalance?.UsedLeaves ?? 0;

		//    // Fetch attendance records for the logged-in user
		//    var attendanceCounts = db.Attendance
		//        .Where(a => a.User.Email == userId)
		//        .GroupBy(a => a.Status)
		//        .Select(g => new { Status = g.Key, Count = g.Count() })
		//        .ToList();

		//    ViewBag.Present = attendanceCounts.FirstOrDefault(a => a.Status == "Present")?.Count ?? 0;
		//    ViewBag.Late = attendanceCounts.FirstOrDefault(a => a.Status == "Late")?.Count ?? 0;
		//    ViewBag.Absent = attendanceCounts.FirstOrDefault(a => a.Status == "Absent")?.Count ?? 0;

		//    // Fetch approved sick leave days
		//    ViewBag.SickLeave = db.LeaveRequests
		//        .Where(l => l.User.Email == userId && l.Reason == "Sick Leave" && l.StatusHistory == "Approved")
		//        .Sum(l => (int?)l.NumberOfDays) ?? 0;

		//    // Fetch total approved leave days
		//    ViewBag.TotalDaysTaken = db.LeaveRequests
		//        .Where(l => l.User.Email == userId && l.Status == "Approved")
		//        .Sum(l => (int?)l.NumberOfDays) ?? 0;

		//    // Fetch the details of the logged-in user
		//    string userEmail = HttpContext.Session.GetString("User");
		//    var user = db.User.Include(u => u.Department).Include(u => u.Designation)
		//                      .SingleOrDefault(u => u.Email == userEmail);

		//    if (user == null)
		//    {
		//        return RedirectToAction("SignIn", "Auth");
		//    }

		//    // Set user details in ViewBag
		//    ViewBag.PhoneNumber = user.PhoneNumber ?? "Not Available";
		//    ViewBag.FirstName = user.FirstName ?? "N/A";
		//    ViewBag.LastName = user.LastName ?? "N/A";
		//    ViewBag.ProfilePicture = user.ProfilePicture ?? "default-profile.png";
		//    ViewBag.Name = user.Designation?.Name ?? "Not Assigned";
		//    ViewBag.Email = user.Email;
		//    ViewBag.ReportOffice = user.Department?.Name ?? "Not Assigned";
		//    ViewBag.JoinedOn = user.DateOfJoining.ToString("dd MMM yyyy") ?? "N/A";

		//    // Attendance handling
		//    var today = DateTime.Today;
		//    //ViewData["LoggedInUser"] = email;
		//    ViewBag.CurrentTime = DateTime.Now.ToString("hh:mm tt");

		//    var attendance = db.Attendance.FirstOrDefault(a => a.Date == today && a.UserId == user.UserId);

		//    if (attendance == null && DateTime.Now.Hour >= 18)
		//    {
		//        attendance = new Attendance
		//        {
		//            Date = today,
		//            UserId = user.UserId,
		//            Status = "Absent"
		//        };
		//        db.Attendance.Add(attendance);
		//        db.SaveChanges();
		//    }

		//    ViewBag.AttendanceStatus = GetAttendanceStatus(attendance);

		//    if (attendance != null)
		//    {
		//        ViewBag.WorkingHours = attendance.WorkingHours > 0 ? attendance.WorkingHours.ToString("0.#") : "0";
		//        ViewBag.BreakHours = attendance.BreakHours > 0 ? attendance.BreakHours.ToString("0.#") : "0";
		//        ViewBag.OvertimeHours = attendance.OvertimeHours > 0 ? attendance.OvertimeHours.ToString("0.#") : "0";
		//        ViewBag.ProductionHours = attendance.ProductionHours > 0 ? attendance.ProductionHours.ToString("0.#") : "0";
		//        ViewBag.LateMinutes = attendance.Late > 0 ? attendance.Late.ToString() : "0";
		//    }
		//    else
		//    {
		//        ViewBag.WorkingHours = "0";
		//        ViewBag.BreakHours = "0";
		//        ViewBag.OvertimeHours = "0";
		//        ViewBag.ProductionHours = "0";
		//        ViewBag.LateMinutes = "0";
		//    }

		//    // Fetch and pass user attendance list
		//    var attendanceList = db.Attendance.Where(a => a.UserId == user.UserId).OrderByDescending(a => a.Date).ToList();
		//    ViewData["AttendanceList"] = attendanceList ?? new List<Attendance>();

		//    // Fetch Project & Task Data
		//    var projectData = db.AllProjects
		//        .OrderByDescending(p => p.ProjectId)
		//        .Take(2)
		//        .Select(p => new
		//        {
		//            p.ProjectName,
		//            p.EndDate,
		//            p.ManagerName,
		//            ManagerProfilePicture = db.User.Where(u => u.UserId.ToString() == p.ManagerName)
		//                                           .Select(u => u.ProfilePicture)
		//                                           .FirstOrDefault(),
		//            TotalTasks = db.Task.Count(t => t.ProjectId == p.ProjectId),
		//            CompletedTasks = db.Task.Count(t => t.ProjectId == p.ProjectId && t.Status == "Completed"),
		//            TaskMembersHtml = string.Join("", db.Taskmember
		//                .Where(tm => db.Task.Any(t => t.ProjectId == p.ProjectId && t.TaskId == tm.TaskId))
		//                .Select(tm => $"<span class='avatar avatar-rounded'><img class='border border-white' src='/{tm.User.ProfilePicture ?? "default-profile.png"}' alt='img'></span>")
		//                .ToList())
		//        })
		//        .ToList();

		//    ViewBag.Project1 = projectData.ElementAtOrDefault(0);
		//    ViewBag.Project2 = projectData.ElementAtOrDefault(1);

		//    // Fetch Recent Tasks
		//    var recentTasks = db.Task
		//        .OrderByDescending(t => t.Deadline)
		//        .Take(5)
		//        .Select(t => new
		//        {
		//            t.TaskId,
		//            t.Title,
		//            t.Status,
		//            t.Priority,
		//            IsCompleted = t.Status == "Completed",
		//            TaskMembers = db.Taskmember
		//                .Where(tm => tm.TaskId == t.TaskId)
		//                .Select(tm => tm.User.ProfilePicture ?? "default-profile.png")
		//                .ToList()
		//        })
		//        .ToList();

		//    ViewBag.RecentTasks = recentTasks;

		//    // Fetch Recent Users
		//    var recentUsers = db.User
		//        .Where(u => u.Role.RoleName != "Admin")
		//        .OrderByDescending(u => u.UserId)
		//        .Take(6)
		//        .Select(u => new
		//        {
		//            FullName = u.FirstName + " " + u.LastName,
		//            ProfilePicture = u.ProfilePicture ?? "default-profile.png",
		//            Designation = u.Designation.Name ?? "Not Assigned"
		//        })
		//        .ToList();

		//    ViewBag.RecentUsers = recentUsers;

		//    return View(attendance ?? new Attendance { Date = today });
		//}

		public IActionResult EmpD()
		{
			string userId = User.Identity.Name; // Assuming the username is stored as User.Identity.Name

			string userEmail = HttpContext.Session.GetString("User");
			var user = db.User.Include(u => u.Department).Include(u => u.Designation)
							  .SingleOrDefault(u => u.Email == userEmail);

			if (user == null)
			{
				return RedirectToAction("SignIn");
			}

			// Pass the user details to the view
			ViewBag.PhoneNumber = user.PhoneNumber;
			ViewBag.FirstName = user.FirstName;
			ViewBag.LastName = user.LastName;
			ViewBag.ProfilePicture = user.ProfilePicture;
			ViewBag.Name = user.Designation.Name;
			ViewBag.Email = user.Email;
			ViewBag.ReportOffice = user.Department.Name;
			ViewBag.JoinedOn = user.DateOfJoining.ToString("dd MMM yyyy");

			// Fetch attendance records for the logged-in user
			var attendanceCounts = db.Attendance
				.Where(a => a.User.Email == userId) // Filter by logged-in user
				.GroupBy(a => a.Status)
				.Select(g => new
				{
					Status = g.Key,
					Count = g.Count()
				})
				.ToList();

			// Initialize variables for each status
			var presentCount = attendanceCounts.FirstOrDefault(a => a.Status == "Present")?.Count ?? 0;
			var lateCount = attendanceCounts.FirstOrDefault(a => a.Status == "Late")?.Count ?? 0;
			var absentCount = attendanceCounts.FirstOrDefault(a => a.Status == "Absent")?.Count ?? 0;

			// Fetch the number of sick leaves for the logged-in user
			var sickLeaveCount = db.LeaveRequests
				.Count(l => l.User.Email == userId && l.MasterLeaveType.LeaveType == "Sick Leave");

			// Pass the data to the view
			ViewBag.Present = presentCount;
			ViewBag.Late = lateCount;
			ViewBag.Absent = absentCount;
			ViewBag.SickLeave = sickLeaveCount;

			//attendance

			var role = User.FindFirstValue(ClaimTypes.Role);
			var email = User.FindFirstValue(ClaimTypes.Name);


			if (string.IsNullOrEmpty(email))
			{
				return RedirectToAction("SignIn", "Auth");
			}

			var users = db.User.SingleOrDefault(x => x.Email == email);
			if (users == null)
			{
				return RedirectToAction("SignIn", "Auth");
			}

			var today = DateTime.Today;


			//var today = new DateTime(2025, 1, 27);


			ViewData["LoggedInUser"] = email;
			ViewBag.CurrentTime = DateTime.Now.ToString("hh:mm tt");

			var attendance = db.Attendance.FirstOrDefault(a => a.Date == today && a.UserId == user.UserId);


			if (attendance == null && DateTime.Now.Hour >= 18)
			{
				attendance = new Attendance
				{
					Date = today,
					UserId = user.UserId,
					Status = "Absent"
				};
				db.Attendance.Add(attendance);
				db.SaveChanges();

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

			decimal totalHoursToday = db.Attendance
			.Where(a => a.Date == today && a.UserId == user.UserId)
			.Sum(a => a.WorkingHours);

			decimal totalHoursWeek = db.Attendance
			 .Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek && a.UserId == user.UserId)
			 .Sum(a => a.WorkingHours);

			decimal totalHoursMonth = db.Attendance
			 .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
			 .Sum(a => a.WorkingHours);

			decimal overtimeThisMonth = db.Attendance
					.Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
					.Sum(a => a.OvertimeHours);

			ViewBag.TotalWorkingHoursToday = totalHoursToday.ToString("0.#");
			ViewBag.TotalWorkingHoursWeek = totalHoursWeek.ToString("0.#");
			ViewBag.TotalWorkingHoursMonth = totalHoursMonth.ToString("0.#");
			ViewBag.OvertimeThisMonth = overtimeThisMonth.ToString("0.#");

			var userInfo = db.User.FirstOrDefault(u => u.UserId == user.UserId);
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

			var attendanceList = db.Attendance.Where(a => a.UserId == user.UserId).OrderByDescending(a => a.Date).ToList();
			ViewData["AttendanceList"] = attendanceList;

			//project and Task
			//var userId = User.Identity.Name; // Assuming the userId is stored in Identity.Name; adjust as needed

			// Fetch projects assigned to the logged-in user
			var projectData = db.AllProjects
				.Where(p => db.Taskmember.Any(tm => db.Task.Any(t => t.ProjectId == p.ProjectId && tm.TaskId == t.TaskId && tm.UserId.ToString() == userId)))
				.OrderByDescending(p => p.ProjectId)
				.Take(2)
				.Select(p => new
				{
					p.ProjectName,
					p.EndDate,
					p.ManagerName,
					ManagerProfilePicture = db.User
						.Where(u => u.UserId.ToString() == p.ManagerName)
						.Select(u => u.ProfilePicture)
						.FirstOrDefault(),
					TotalTasks = db.Task.Count(t => t.ProjectId == p.ProjectId),
					CompletedTasks = db.Task.Count(t => t.ProjectId == p.ProjectId && t.Status == "Completed"),
					TaskMembersHtml = string.Join("", db.Taskmember
						.Where(tm => db.Task.Any(t => t.ProjectId == p.ProjectId && t.TaskId == tm.TaskId))
						.Select(tm => $"<span class='avatar avatar-rounded'><img class='border border-white' src='/{tm.User.ProfilePicture}' alt='img'></span>")
						.ToList())
				})
				.ToList();

			// Fetch recent tasks assigned to the logged-in user
			var recentTasks = db.Task
				.Where(t => db.Taskmember.Any(tm => tm.TaskId == t.TaskId && tm.UserId.ToString() == userId))
				.OrderByDescending(t => t.Deadline)
				.Take(5)
				.Select(t => new
				{
					t.TaskId,
					t.Title,
					t.Status,
					t.Priority,
					IsCompleted = t.Status == "Completed",
					TaskMembers = db.Taskmember
						.Where(tm => tm.TaskId == t.TaskId)
						.Select(tm => tm.User.ProfilePicture)
						.ToList()
				})
				.ToList();

			// Fetch recent users (unchanged, as it’s not user-specific)
			var recentUsers = db.User
				.Where(u => u.Role.RoleName != "Admin")
				.OrderByDescending(u => u.UserId)
				.Take(6)
				.Select(u => new
				{
					FullName = u.FirstName + " " + u.LastName,
					u.ProfilePicture,
					Designation = u.Designation != null ? u.Designation.Name : "Not Assigned"
				})
				.ToList();

			// Check if user has any projects or tasks assigned
			if (projectData.Any() || recentTasks.Any())
			{
				ViewBag.Project1 = projectData.ElementAtOrDefault(0);
				ViewBag.Project2 = projectData.ElementAtOrDefault(1);
				ViewBag.RecentTasks = recentTasks;
				ViewBag.RecentUsers = recentUsers;

				return View();
			}
			else
			{
				return View();
			}


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
			var user = db.User.Include(x => x.Role).SingleOrDefault(x => x.Email == email);

			if (user == null)
			{
				return RedirectToAction("SignIn", "Auth");
			}

			var today = DateTime.Today;
			//var today =new DateTime(2025, 1, 27);

			//var today = new DateTime(2025, 1, 27, 18, 40, 0);

			var attendance = db.Attendance.FirstOrDefault(a => a.Date == today && a.UserId == user.UserId);

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
				db.Attendance.Add(attendance);
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



					db.SaveChanges();
					db.Entry(attendance).Reload();

				

					//_context.Entry(attendance).State = EntityState.Modified;
				}
			}

			//if (attendance.CheckIn == null && attendance.LunchIn == null && attendance.LunchOut == null && attendance.CheckOut == null)
			//{
			//    attendance.Status = "Absent";
			//}

			try
			{
				db.SaveChanges();
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




		[Authorize(Roles = "Manager")]
		public IActionResult ManagerD()
		{

            string userId = User.Identity.Name; // Assuming the username is stored as User.Identity.Name

            string userEmail = HttpContext.Session.GetString("User");
            var user = db.User.Include(u => u.Department).Include(u => u.Designation)
                              .SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("SignIn");
            }

            // Pass the user details to the view
            ViewBag.PhoneNumber = user.PhoneNumber;
            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.ProfilePicture = user.ProfilePicture;
            ViewBag.Name = user.Designation.Name;
            ViewBag.Email = user.Email;
            ViewBag.ReportOffice = user.Department.Name;
            ViewBag.JoinedOn = user.DateOfJoining.ToString("dd MMM yyyy");

            // Fetch attendance records for the logged-in user
            var attendanceCounts = db.Attendance
                .Where(a => a.User.Email == userId) // Filter by logged-in user
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Initialize variables for each status
            var presentCount = attendanceCounts.FirstOrDefault(a => a.Status == "Present")?.Count ?? 0;
            var lateCount = attendanceCounts.FirstOrDefault(a => a.Status == "Late")?.Count ?? 0;
            var absentCount = attendanceCounts.FirstOrDefault(a => a.Status == "Absent")?.Count ?? 0;

            // Fetch the number of sick leaves for the logged-in user
            var sickLeaveCount = db.LeaveRequests
                .Count(l => l.User.Email == userId && l.MasterLeaveType.LeaveType == "Sick Leave");

            // Pass the data to the view
            ViewBag.Present = presentCount;
            ViewBag.Late = lateCount;
            ViewBag.Absent = absentCount;
            ViewBag.SickLeave = sickLeaveCount;

            //attendance

            var role = User.FindFirstValue(ClaimTypes.Role);
            var email = User.FindFirstValue(ClaimTypes.Name);


            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var users = db.User.SingleOrDefault(x => x.Email == email);
            if (users == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var today = DateTime.Today;


            //var today = new DateTime(2025, 1, 27);


            ViewData["LoggedInUser"] = email;
            ViewBag.CurrentTime = DateTime.Now.ToString("hh:mm tt");

            var attendance = db.Attendance.FirstOrDefault(a => a.Date == today && a.UserId == user.UserId);


            if (attendance == null && DateTime.Now.Hour >= 18)
            {
                attendance = new Attendance
                {
                    Date = today,
                    UserId = user.UserId,
                    Status = "Absent"
                };
                db.Attendance.Add(attendance);
                db.SaveChanges();

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

            decimal totalHoursToday = db.Attendance
            .Where(a => a.Date == today && a.UserId == user.UserId)
            .Sum(a => a.WorkingHours);

            decimal totalHoursWeek = db.Attendance
             .Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek && a.UserId == user.UserId)
             .Sum(a => a.WorkingHours);

            decimal totalHoursMonth = db.Attendance
             .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
             .Sum(a => a.WorkingHours);

            decimal overtimeThisMonth = db.Attendance
                    .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
                    .Sum(a => a.OvertimeHours);

            ViewBag.TotalWorkingHoursToday = totalHoursToday.ToString("0.#");
            ViewBag.TotalWorkingHoursWeek = totalHoursWeek.ToString("0.#");
            ViewBag.TotalWorkingHoursMonth = totalHoursMonth.ToString("0.#");
            ViewBag.OvertimeThisMonth = overtimeThisMonth.ToString("0.#");

            var userInfo = db.User.FirstOrDefault(u => u.UserId == user.UserId);
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

            var attendanceList = db.Attendance.Where(a => a.UserId == user.UserId).OrderByDescending(a => a.Date).ToList();
            ViewData["AttendanceList"] = attendanceList;

            //project and Task
            //var userId = User.Identity.Name; // Assuming the userId is stored in Identity.Name; adjust as needed

            // Fetch projects assigned to the logged-in user
            var projectData = db.AllProjects
                .Where(p => db.Taskmember.Any(tm => db.Task.Any(t => t.ProjectId == p.ProjectId && tm.TaskId == t.TaskId && tm.UserId.ToString() == userId)))
                .OrderByDescending(p => p.ProjectId)
                .Take(2)
                .Select(p => new
                {
                    p.ProjectName,
                    p.EndDate,
                    p.ManagerName,
                    ManagerProfilePicture = db.User
                        .Where(u => u.UserId.ToString() == p.ManagerName)
                        .Select(u => u.ProfilePicture)
                        .FirstOrDefault(),
                    TotalTasks = db.Task.Count(t => t.ProjectId == p.ProjectId),
                    CompletedTasks = db.Task.Count(t => t.ProjectId == p.ProjectId && t.Status == "Completed"),
                    TaskMembersHtml = string.Join("", db.Taskmember
                        .Where(tm => db.Task.Any(t => t.ProjectId == p.ProjectId && t.TaskId == tm.TaskId))
                        .Select(tm => $"<span class='avatar avatar-rounded'><img class='border border-white' src='/{tm.User.ProfilePicture}' alt='img'></span>")
                        .ToList())
                })
                .ToList();

            // Fetch recent tasks assigned to the logged-in user
            var recentTasks = db.Task
                .Where(t => db.Taskmember.Any(tm => tm.TaskId == t.TaskId && tm.UserId.ToString() == userId))
                .OrderByDescending(t => t.Deadline)
                .Take(5)
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    t.Status,
                    t.Priority,
                    IsCompleted = t.Status == "Completed",
                    TaskMembers = db.Taskmember
                        .Where(tm => tm.TaskId == t.TaskId)
                        .Select(tm => tm.User.ProfilePicture)
                        .ToList()
                })
                .ToList();

            // Fetch recent users (unchanged, as it’s not user-specific)
            var recentUsers = db.User
                .Where(u => u.Role.RoleName != "Admin")
                .OrderByDescending(u => u.UserId)
                .Take(6)
                .Select(u => new
                {
                    FullName = u.FirstName + " " + u.LastName,
                    u.ProfilePicture,
                    Designation = u.Designation != null ? u.Designation.Name : "Not Assigned"
                })
                .ToList();

            // Check if user has any projects or tasks assigned
            if (projectData.Any() || recentTasks.Any())
            {
                ViewBag.Project1 = projectData.ElementAtOrDefault(0);
                ViewBag.Project2 = projectData.ElementAtOrDefault(1);
                ViewBag.RecentTasks = recentTasks;
                ViewBag.RecentUsers = recentUsers;

                return View();
            }
            else
            {
                return View();
            }


        }

        [Authorize(Roles = "HR")]
		public IActionResult HRD()
		{

            string userId = User.Identity.Name; // Assuming the username is stored as User.Identity.Name

            string userEmail = HttpContext.Session.GetString("User");
            var user = db.User.Include(u => u.Department).Include(u => u.Designation)
                              .SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("SignIn");
            }

            // Pass the user details to the view
            ViewBag.PhoneNumber = user.PhoneNumber;
            ViewBag.FirstName = user.FirstName;
            ViewBag.LastName = user.LastName;
            ViewBag.ProfilePicture = user.ProfilePicture;
            ViewBag.Name = user.Designation.Name;
            ViewBag.Email = user.Email;
            ViewBag.ReportOffice = user.Department.Name;
            ViewBag.JoinedOn = user.DateOfJoining.ToString("dd MMM yyyy");

            // Fetch attendance records for the logged-in user
            var attendanceCounts = db.Attendance
                .Where(a => a.User.Email == userId) // Filter by logged-in user
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Initialize variables for each status
            var presentCount = attendanceCounts.FirstOrDefault(a => a.Status == "Present")?.Count ?? 0;
            var lateCount = attendanceCounts.FirstOrDefault(a => a.Status == "Late")?.Count ?? 0;
            var absentCount = attendanceCounts.FirstOrDefault(a => a.Status == "Absent")?.Count ?? 0;

            // Fetch the number of sick leaves for the logged-in user
            var sickLeaveCount = db.LeaveRequests
                .Count(l => l.User.Email == userId && l.MasterLeaveType.LeaveType == "Sick Leave");

            // Pass the data to the view
            ViewBag.Present = presentCount;
            ViewBag.Late = lateCount;
            ViewBag.Absent = absentCount;
            ViewBag.SickLeave = sickLeaveCount;

            //attendance

            var role = User.FindFirstValue(ClaimTypes.Role);
            var email = User.FindFirstValue(ClaimTypes.Name);


            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var users = db.User.SingleOrDefault(x => x.Email == email);
            if (users == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var today = DateTime.Today;


            //var today = new DateTime(2025, 1, 27);


            ViewData["LoggedInUser"] = email;
            ViewBag.CurrentTime = DateTime.Now.ToString("hh:mm tt");

            var attendance = db.Attendance.FirstOrDefault(a => a.Date == today && a.UserId == user.UserId);


            if (attendance == null && DateTime.Now.Hour >= 18)
            {
                attendance = new Attendance
                {
                    Date = today,
                    UserId = user.UserId,
                    Status = "Absent"
                };
                db.Attendance.Add(attendance);
                db.SaveChanges();

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

            decimal totalHoursToday = db.Attendance
            .Where(a => a.Date == today && a.UserId == user.UserId)
            .Sum(a => a.WorkingHours);

            decimal totalHoursWeek = db.Attendance
             .Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek && a.UserId == user.UserId)
             .Sum(a => a.WorkingHours);

            decimal totalHoursMonth = db.Attendance
             .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
             .Sum(a => a.WorkingHours);

            decimal overtimeThisMonth = db.Attendance
                    .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth && a.UserId == user.UserId)
                    .Sum(a => a.OvertimeHours);

            ViewBag.TotalWorkingHoursToday = totalHoursToday.ToString("0.#");
            ViewBag.TotalWorkingHoursWeek = totalHoursWeek.ToString("0.#");
            ViewBag.TotalWorkingHoursMonth = totalHoursMonth.ToString("0.#");
            ViewBag.OvertimeThisMonth = overtimeThisMonth.ToString("0.#");

            var userInfo = db.User.FirstOrDefault(u => u.UserId == user.UserId);
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

            var attendanceList = db.Attendance.Where(a => a.UserId == user.UserId).OrderByDescending(a => a.Date).ToList();
            ViewData["AttendanceList"] = attendanceList;

            //project and Task
            //var userId = User.Identity.Name; // Assuming the userId is stored in Identity.Name; adjust as needed

            // Fetch projects assigned to the logged-in user
            var projectData = db.AllProjects
                .Where(p => db.Taskmember.Any(tm => db.Task.Any(t => t.ProjectId == p.ProjectId && tm.TaskId == t.TaskId && tm.UserId.ToString() == userId)))
                .OrderByDescending(p => p.ProjectId)
                .Take(2)
                .Select(p => new
                {
                    p.ProjectName,
                    p.EndDate,
                    p.ManagerName,
                    ManagerProfilePicture = db.User
                        .Where(u => u.UserId.ToString() == p.ManagerName)
                        .Select(u => u.ProfilePicture)
                        .FirstOrDefault(),
                    TotalTasks = db.Task.Count(t => t.ProjectId == p.ProjectId),
                    CompletedTasks = db.Task.Count(t => t.ProjectId == p.ProjectId && t.Status == "Completed"),
                    TaskMembersHtml = string.Join("", db.Taskmember
                        .Where(tm => db.Task.Any(t => t.ProjectId == p.ProjectId && t.TaskId == tm.TaskId))
                        .Select(tm => $"<span class='avatar avatar-rounded'><img class='border border-white' src='/{tm.User.ProfilePicture}' alt='img'></span>")
                        .ToList())
                })
                .ToList();

            // Fetch recent tasks assigned to the logged-in user
            var recentTasks = db.Task
                .Where(t => db.Taskmember.Any(tm => tm.TaskId == t.TaskId && tm.UserId.ToString() == userId))
                .OrderByDescending(t => t.Deadline)
                .Take(5)
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    t.Status,
                    t.Priority,
                    IsCompleted = t.Status == "Completed",
                    TaskMembers = db.Taskmember
                        .Where(tm => tm.TaskId == t.TaskId)
                        .Select(tm => tm.User.ProfilePicture)
                        .ToList()
                })
                .ToList();

            // Fetch recent users (unchanged, as it’s not user-specific)
            var recentUsers = db.User
                .Where(u => u.Role.RoleName != "Admin")
                .OrderByDescending(u => u.UserId)
                .Take(6)
                .Select(u => new
                {
                    FullName = u.FirstName + " " + u.LastName,
                    u.ProfilePicture,
                    Designation = u.Designation != null ? u.Designation.Name : "Not Assigned"
                })
                .ToList();

            // Check if user has any projects or tasks assigned
            if (projectData.Any() || recentTasks.Any())
            {
                ViewBag.Project1 = projectData.ElementAtOrDefault(0);
                ViewBag.Project2 = projectData.ElementAtOrDefault(1);
                ViewBag.RecentTasks = recentTasks;
                ViewBag.RecentUsers = recentUsers;

                return View();
            }
            else
            {
                return View();
            }


        }


        [HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
			HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			var all = Request.Cookies.Keys;
			HttpContext.Session.Clear();
			foreach (var cookie in all)
			{
				Response.Cookies.Delete(cookie);
			}
			return RedirectToAction("SignIn");
		}

		[HttpPost]
		public IActionResult GoogleLogin()
		{
			var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
			return Challenge(properties, GoogleDefaults.AuthenticationScheme);
		}

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims
                .Select(c => new
                {
                    c.Type,
                    c.Value
                }).ToList();

            var email = claims?.FirstOrDefault(x => x.Type.Contains("email"))?.Value;
            var name = claims?.FirstOrDefault(x => x.Type.Contains("name"))?.Value;

            if (email == null)
            {
                TempData["error"] = "Google authentication failed. Email not found.";
                return RedirectToAction("SignIn");
            }

            // Check if the email exists in the registered users
            var user = db.User.Include(u => u.Role).FirstOrDefault(x => x.Email == email);

            if (user != null)
            {
                // Create claims
                var identity = new ClaimsIdentity(new[]
                {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Role, user.Role.RoleName),
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Set session data
                HttpContext.Session.SetString("User", user.Email);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Epath", user.ProfilePicture);

                // Redirect based on role
                return user.Role.RoleName switch
                {
                    "Admin" => RedirectToAction("AdminD"),
                    "Employee" => RedirectToAction("EmpD"),
                    "Manager" => RedirectToAction("ManagerD"),
                    "Super Admin" => RedirectToAction("SuperAdminD", "Dashboard"),
                    _ => RedirectToAction("SignIn")
                };
            }
            else
            {
                TempData["error"] = "No registered user found with this Google email.";
                return RedirectToAction("SignIn");
            }
        }

        //public async Task<IActionResult> Logout()
        //{
        //	await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //	return RedirectToAction("Login");
        //}

        // REPORT SECTION 
        public IActionResult EmpByDeptGraph()
		{
			return View();
		}

		public IActionResult EmpByDeptGraphData()
		{
			var data = db.User
				.GroupBy(u => u.Department.Name)
				.Select(g => new
				{
					DepartmentName = g.Key,
					EmployeeCount = g.Count()
				})
				.ToList();

			return Json(data);
		}

		public IActionResult TasksStats()
		{
			// Total tasks
			//int totalTasks = db.Tasks.Count();

			//if (totalTasks == 0)
			//{
			//    ViewBag.InProgress = 0;
			//    ViewBag.OnHold = 0;
			//    ViewBag.Overdue = 0;
			//    ViewBag.Completed = 0;
			//    ViewBag.TotalTasks = "0/0";
			//    ViewBag.InProgressPercent = 0;
			//    ViewBag.OnHoldPercent = 0;
			//    ViewBag.OverduePercent = 0;
			//    ViewBag.CompletedPercent = 0;
			//    return View();
			//}

			//// Task counts
			//int inProgressCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "inprogress");
			//int onHoldCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "onhold");
			//int overdueCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "overdue");
			//int completedCount = db.Tasks.Count(t => t.Status.Trim().ToLower() == "completed");

			//ViewBag.InProgress = inProgressCount;
			//ViewBag.OnHold = onHoldCount;
			//ViewBag.Overdue = overdueCount;
			//ViewBag.Completed = completedCount;
			//ViewBag.TotalTasks = $"{completedCount}/{totalTasks}";
			//int total = (int)totalTasks;

			//// Calculate percentages
			//ViewBag.InProgressPercent = (inProgressCount * 100) / totalTasks;
			//ViewBag.OnHoldPercent = (onHoldCount * 100) / totalTasks;
			//ViewBag.OverduePercent = (overdueCount * 100) / totalTasks;
			//ViewBag.CompletedPercent = (completedCount * 100) / totalTasks;

			return View();
		}

		//Admin Dashboard cards
		public IActionResult Index()
		{
			//Attendance 
			int totalEmployees = db.User.Count();
			int presentCount = db.Attendance.Count(a => a.Status == "Present");
			ViewBag.PresentRatio = $"{presentCount}/{totalEmployees}";

			//Project 
			int totalProjects = db.AllProjects.Count();
			int activeProjects = db.AllProjects.Count(p => p.Status == "Active");
			ViewBag.ProjectStatusRatio = $"{activeProjects}/{totalProjects}";


			//Client
			int totalClients = db.AllProjects
								 .Select(p => p.ClientName)
								 .Distinct()
								 .Count();
			int activeClients = db.AllProjects
								  .Where(p => p.Status == "Active")
								  .Select(p => p.ClientName)
								  .Distinct()
								  .Count();
			ViewBag.ClientStatusRatio = $"{activeClients}/{totalClients}";


			ViewBag.TotalClients = totalClients;

			//Task
			int totalTasks = db.Task.Count();
			ViewBag.TotalTasks = totalTasks;

			//Earnings
			var totalEarnings = db.AllProjects.Sum(p => p.ProjectValue);
			var formattedTotalEarnings = totalEarnings.ToString("C0", CultureInfo.GetCultureInfo("en-US"));
			ViewBag.TotalEarnings = formattedTotalEarnings;


			return View();
		}

		//Fetching Holiday List
		public IActionResult NextHoliday()
		{
			var eventTypes = db.Events.ToList();
			return View(eventTypes);
		}



       

    }
}


    