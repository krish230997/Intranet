using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using System.Security.Claims;

namespace Pulse360.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext db;
        public LeaveController(ApplicationDbContext db)
        {
            this.db = db;
        }
        // GET: AddLeaveType
        public IActionResult AddLeaveType()
        {
            if (User.Identity.IsAuthenticated)
            {
                var role = User.FindFirstValue(ClaimTypes.Role);

                if (role != "Admin")
                {
                    TempData["error"] = "You do not have permission to add leave types.";
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }

            ViewBag.LeaveTypes = db.MasterLeaveTypes
                .Select(lt => new
                {
                    lt.LeaveTypeId,
                    lt.LeaveType,
                    Status = db.DepartmentLeaves
                        .Where(dl => dl.LeaveTypeId == lt.LeaveTypeId)
                        .Select(dl => dl.Status)
                        .FirstOrDefault() ?? "Active"
                })
                .ToList();

            return View();
        }

        // POST: AddLeaveType
        [HttpPost]
        public IActionResult AddLeaveType(string LeaveType)
        {
            if (string.IsNullOrWhiteSpace(LeaveType))
            {
                TempData["error"] = "Leave type cannot be empty!";
                return RedirectToAction("AddLeaveType");
            }

            var existingLeaveType = db.MasterLeaveTypes
                .FirstOrDefault(l => l.LeaveType.ToLower() == LeaveType.ToLower());

            if (existingLeaveType != null)
            {
                TempData["error"] = "Leave type already exists!";
                return RedirectToAction("AddLeaveType");
            }

            var newLeaveType = new MasterLeaveType { LeaveType = LeaveType };
            db.MasterLeaveTypes.Add(newLeaveType);
            db.SaveChanges();

            TempData["success"] = "Leave type added successfully!";
            return RedirectToAction("AddLeaveType");
        }

        //// POST: EditLeaveType
        //[HttpPost]
        //public IActionResult EditLeaveType(int leaveTypeId, string newLeaveType)
        //{
        //    var leaveType = db.MasterLeaveTypes.Find(leaveTypeId);
        //    if (leaveType == null)
        //    {
        //        TempData["error"] = "Leave type not found.";
        //        return RedirectToAction("AddLeaveType");
        //    }

        //    leaveType.LeaveType = newLeaveType;
        //    db.MasterLeaveTypes.Update(leaveType);
        //    db.SaveChanges();

        //    TempData["success"] = "Leave type updated successfully!";
        //    return RedirectToAction("AddLeaveType");
        //}

        // POST: DeleteLeaveType
        //[HttpPost]
        //public IActionResult DeleteLeaveType(int leaveTypeId)
        //{
        //    var leaveType = db.MasterLeaveTypes.Find(leaveTypeId);
        //    if (leaveType == null)
        //    {
        //        TempData["error"] = "Leave type not found.";
        //        return RedirectToAction("AddLeaveType");
        //    }

        //    db.MasterLeaveTypes.Remove(leaveType);
        //    db.SaveChanges();

        //    TempData["success"] = "Leave type deleted successfully!";
        //    return RedirectToAction("AddLeaveType");
        //}

        [HttpPost]
        public IActionResult DeleteLeaveType(int leaveTypeId)
        {
            try
            {
                var leaveType = db.MasterLeaveTypes.Find(leaveTypeId);
                if (leaveType == null)
                {
                    TempData["error"] = "Leave type not found.";
                    return RedirectToAction("AddLeaveType");
                }

                db.MasterLeaveTypes.Remove(leaveType);
                db.SaveChanges();

                TempData["success"] = "Leave type deleted successfully!";
            }
            catch (DbUpdateException ex)
            {
                TempData["error"] = "Cannot delete this leave type because it is being used in another record.";
                TempData.Keep("error");
                return RedirectToAction("AddLeaveType"); // Redirect instead of returning View()
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the leave type.";
                return View();    
            }
            //return View();

            return RedirectToAction("AddLeaveType");
        }

        // GET: AddLeaveDeptwise
        public IActionResult AddLeaveDeptwise()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");
                ViewBag.LeaveTypes = new SelectList(db.MasterLeaveTypes, "LeaveTypeId", "LeaveType");

                return View();
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }

        [HttpPost]
        public IActionResult AddLeaveDeptwise(int departmentId, int leaveTypeId, int leavesCount)
        {
            var leaveTypeStatus = db.DepartmentLeaves
                .Where(dl => dl.LeaveTypeId == leaveTypeId)
                .Select(dl => dl.Status)
                .FirstOrDefault();

            if (leaveTypeStatus == "Inactive")
            {
                TempData["error"] = "The selected leave type is deactivated and cannot be allocated.";
                return RedirectToAction("AddLeaveDeptwise");
            }
            var departmentLeave = db.DepartmentLeaves
                .FirstOrDefault(dl => dl.DepartmentId == departmentId && dl.LeaveTypeId == leaveTypeId);

            if (departmentLeave == null)
            {
                departmentLeave = new DepartmentLeaves
                {
                    DepartmentId = departmentId,
                    LeaveTypeId = leaveTypeId,
                    LeavesCount = leavesCount,
                    Status = "Active"
                };
                db.DepartmentLeaves.Add(departmentLeave);
            }
            else
            {
                departmentLeave.LeavesCount += leavesCount;
                db.DepartmentLeaves.Update(departmentLeave);
            }

            db.SaveChanges();

            var departmentEmployees = db.User.Where(u => u.DepartmentId == departmentId).ToList();
            foreach (var employee in departmentEmployees)
            {
                var leaveBalance = db.LeaveBalances
                    .FirstOrDefault(lb => lb.UserId == employee.UserId && lb.LeaveTypeId == leaveTypeId);

                if (leaveBalance == null)
                {
                    leaveBalance = new LeaveBalance
                    {
                        UserId = employee.UserId,
                        DepartmentLeavesId = departmentLeave.DepartmentLeavesId,
                        LeaveTypeId = leaveTypeId,
                        TotalLeaves = leavesCount,
                        UsedLeaves = 0
                    };
                    db.LeaveBalances.Add(leaveBalance);
                }
                else
                {
                    leaveBalance.TotalLeaves += leavesCount;
                    db.LeaveBalances.Update(leaveBalance);
                }
            }

            db.SaveChanges();

            TempData["success"] = "Leave allocation for the department has been updated successfully!";
            return RedirectToAction("AddLeaveDeptwise");
        }

        //GET DepartmentLeaveDetails
        public IActionResult DepartmentLeaveDetails()
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Employee")))
            {
                ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");

                var departmentLeaves = db.DepartmentLeaves
                    .Include(dl => dl.Department)
                    .Include(dl => dl.MasterLeaveType)
                    .ToList();

                return View(departmentLeaves);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }
        [HttpPost]
        public IActionResult DepartmentLeaveDetails(int departmentId)
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Employee")))
            {
                ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");
               

                var departmentLeaves = db.DepartmentLeaves
                    .Include(dl => dl.Department)
                    .Include(dl => dl.MasterLeaveType)
                    .Where(dl => dl.DepartmentId == departmentId)
                    .ToList();

                return View(departmentLeaves);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }


        // GET: ManageLeaveSettings
        public IActionResult ManageLeaveSettings()
        {
            if (User.IsInRole("Admin"))
            {
                var leaveSettings = db.MasterLeaveTypes
                    .Select(mlt => new
                    {
                        LeaveTypeId = mlt.LeaveTypeId,
                        LeaveType = mlt.LeaveType,

                        Status = db.DepartmentLeaves
                            .Where(dl => dl.LeaveTypeId == mlt.LeaveTypeId)
                            .Select(dl => dl.Status)
                            .FirstOrDefault() ?? "Active"
                    })
                    .ToList();

                return View(leaveSettings);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }
        [HttpPost]
        public IActionResult UpdateLeaveTypeStatus(int leaveTypeId, string status)
        {
            // Retrieve all department leaves related to this leave type
            var departmentLeaves = db.DepartmentLeaves.Where(dl => dl.LeaveTypeId == leaveTypeId).ToList();

            if (departmentLeaves.Any())
            {
                // Update status for all department leaves where the leave type is found
                foreach (var dl in departmentLeaves)
                {
                    dl.Status = status;
                }
            }
            else
            {
                // In case no department leaves were found, ensure all departments have this leave type with the selected status
                var departments = db.Departments.ToList();
                foreach (var department in departments)
                {
                    db.DepartmentLeaves.Add(new DepartmentLeaves
                    {
                        DepartmentId = department.DepartmentId,
                        LeaveTypeId = leaveTypeId,
                        LeavesCount = 0, // Set initial count to 0 when adding new department leaves
                        Status = status
                    });
                }
            }

            // Save changes to the database
            db.SaveChanges();

            // Return success message
            TempData["success"] = $"Leave type status has been updated to {status} successfully!";
            return RedirectToAction("ManageLeaveSettings");
        }

        private int? GetCurrentUserId()
        {
            if (HttpContext.Session.GetInt32("UserId") is int userId)
            {
                return userId;
            }
            if (Request.Cookies.TryGetValue("UserId", out var userIdStr) && int.TryParse(userIdStr, out var userIdFromCookie))
            {
                return userIdFromCookie;
            }

            return null;
        }

        // // GET: ApplyLeave
        public IActionResult ApplyLeave()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Employee")
            {
                TempData["error"] = "You do not have permission to perform this action.";
                return RedirectToAction("ApplyLeave", "Leave");

            }
            var email = User.FindFirstValue(ClaimTypes.Name);

            var user = db.User.Include(x => x.Role).SingleOrDefault(x => x.Email == email);

            //var userId = GetCurrentUserId();
            if (User.IsInRole("Employee"))
            {
                //if (!userId.HasValue)
                //{
                //	TempData["Message"] = "Session expired. Please log in again.";
                //	return RedirectToAction("SignIn", "Auth");
                //}

                var leaveBalances = db.LeaveBalances
                    .Where(lb => lb.User.Email == email)
                    .Include(lb => lb.MasterLeaveType)
                    .ToList();

                var model = leaveBalances.Select(lb => new
                {
                    lb.MasterLeaveType.LeaveType,
                    lb.MasterLeaveType.LeaveTypeId,
                    lb.TotalLeaves,
                    lb.RemainingLeaves
                }).ToList();
                return View(model);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");

            }
        }

        //[HttpPost]
        //public IActionResult ApplyLeave(LeaveRequest model)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("SignIn", "Auth");
        //    }

        //    var role = User.FindFirstValue(ClaimTypes.Role);

        //    if (role != "Employee")
        //    {
        //        TempData["error"] = "You do not have permission to perform this action.";
        //        return RedirectToAction("ApplyLeave", "Leave");
        //    }

        //    var email = User.FindFirstValue(ClaimTypes.Name);
        //    var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        //    if (model.LeaveTypeId == 0)
        //    {
        //        TempData["error"] = "Please select a leave type.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    var leaveTypeStatus = db.DepartmentLeaves
        //        .Where(dl => dl.LeaveTypeId == model.LeaveTypeId)
        //        .Select(dl => dl.Status)
        //        .FirstOrDefault();

        //    if (leaveTypeStatus == "Inactive")
        //    {
        //        TempData["error"] = "The selected leave type is inactive and cannot be applied for.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    var leaveBalance = db.LeaveBalances
        //        .FirstOrDefault(lb => lb.User.Email == email && lb.LeaveTypeId == model.LeaveTypeId);

        //    if (leaveBalance == null)
        //    {
        //        TempData["error"] = "Leave balance not found for the selected leave type.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    if (model.EndDate < model.StartDate)
        //    {
        //        TempData["error"] = "End date cannot be earlier than start date.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    int numberOfDays = (model.EndDate - model.StartDate).Days + 1;

        //    if (numberOfDays <= 0)
        //    {
        //        TempData["error"] = "Invalid leave duration.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    if (leaveBalance.RemainingLeaves < numberOfDays)
        //    {
        //        TempData["error"] = $"Insufficient leave balance. You only have {leaveBalance.RemainingLeaves} leave days remaining.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    // Deduct the leave by increasing the UsedLeaves count
        //    leaveBalance.UsedLeaves += numberOfDays;

        //    // Set model properties
        //    model.UserId = userId;
        //    model.Status = "Pending";
        //    model.ApprovedBy = string.Empty;
        //    model.StatusHistory = string.Empty;
        //    model.NumberOfDays = numberOfDays;

        //    db.LeaveRequests.Add(model);
        //    db.SaveChanges();

        //    TempData["success"] = "Your leave request has been sent for approval.";
        //    return RedirectToAction("ApplyLeave");
        //}

        //[HttpPost]
        //public IActionResult ApplyLeave(LeaveRequest model)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("SignIn", "Auth");
        //    }

        //    var role = User.FindFirstValue(ClaimTypes.Role);

        //    if (role != "Employee")
        //    {
        //        TempData["error"] = "You do not have permission to perform this action.";
        //        return RedirectToAction("ApplyLeave", "Leave");
        //    }

        //    var email = User.FindFirstValue(ClaimTypes.Name);
        //    var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        //    if (model.LeaveTypeId == 0)
        //    {
        //        TempData["error"] = "Please select a leave type.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    var leaveTypeStatus = db.DepartmentLeaves
        //        .Where(dl => dl.LeaveTypeId == model.LeaveTypeId)
        //        .Select(dl => dl.Status)
        //        .FirstOrDefault();

        //    if (leaveTypeStatus == "Inactive")
        //    {
        //        TempData["error"] = "The selected leave type is inactive and cannot be applied for.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    var leaveBalance = db.LeaveBalances
        //        .FirstOrDefault(lb => lb.User.Email == email && lb.LeaveTypeId == model.LeaveTypeId);

        //    if (leaveBalance == null)
        //    {
        //        TempData["error"] = "Leave balance not found for the selected leave type.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    if (model.EndDate < model.StartDate)
        //    {
        //        TempData["error"] = "End date cannot be earlier than start date.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    int numberOfDays = (model.EndDate - model.StartDate).Days + 1;

        //    if (numberOfDays <= 0)
        //    {
        //        TempData["error"] = "Invalid leave duration.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    if (leaveBalance.RemainingLeaves < numberOfDays)
        //    {
        //        TempData["error"] = $"Insufficient leave balance. You only have {leaveBalance.RemainingLeaves} leave days remaining.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    // **DO NOT Deduct leave balance here** - It should only be deducted upon approval

        //    // Set model properties
        //    model.UserId = userId;
        //    model.Status = "Pending";
        //    model.ApprovedBy = string.Empty;
        //    model.StatusHistory = string.Empty;
        //    model.NumberOfDays = numberOfDays;

        //    db.LeaveRequests.Add(model);
        //    db.SaveChanges();

        //    TempData["success"] = "Your leave request has been sent for approval.";
        //    return RedirectToAction("ApplyLeave");
        //}


        //[HttpPost]
        //public IActionResult ApplyLeave(LeaveRequest model)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("SignIn", "Auth");
        //    }

        //    var role = User.FindFirstValue(ClaimTypes.Role);

        //    if (role != "Employee")
        //    {
        //        TempData["error"] = "You do not have permission to perform this action.";
        //        return RedirectToAction("ApplyLeave", "Leave");
        //    }

        //    var email = User.FindFirstValue(ClaimTypes.Name);
        //    var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        //    if (model.LeaveTypeId == 0)
        //    {
        //        TempData["error"] = "Please select a leave type.";
        //        return RedirectToAction("ApplyLeave");
        //    }

        //    // Check if the selected leave type is an active holiday
        //    var holidayEvent = db.Events
        //        .FirstOrDefault(e => e.EventTypeId == model.LeaveTypeId && e.Status == "Active" && e.Title == "Holiday");

        //    bool isHoliday = holidayEvent != null;

        //    if (!isHoliday)
        //    {
        //        var leaveTypeStatus = db.DepartmentLeaves
        //            .Where(dl => dl.LeaveTypeId == model.LeaveTypeId)
        //            .Select(dl => dl.Status)
        //            .FirstOrDefault();

        //        if (leaveTypeStatus == "Inactive")
        //        {
        //            TempData["error"] = "The selected leave type is inactive and cannot be applied for.";
        //            return RedirectToAction("ApplyLeave");
        //        }

        //        var leaveBalance = db.LeaveBalances
        //            .FirstOrDefault(lb => lb.User.Email == email && lb.LeaveTypeId == model.LeaveTypeId);

        //        if (leaveBalance == null)
        //        {
        //            TempData["error"] = "Leave balance not found for the selected leave type.";
        //            return RedirectToAction("ApplyLeave");
        //        }

        //        if (model.EndDate < model.StartDate)
        //        {
        //            TempData["error"] = "End date cannot be earlier than start date.";
        //            return RedirectToAction("ApplyLeave");
        //        }

        //        int numberOfDays = (model.EndDate - model.StartDate).Days + 1;

        //        if (numberOfDays <= 0)
        //        {
        //            TempData["error"] = "Invalid leave duration.";
        //            return RedirectToAction("ApplyLeave");
        //        }

        //        if (leaveBalance.RemainingLeaves < numberOfDays)
        //        {
        //            TempData["error"] = $"Insufficient leave balance. You only have {leaveBalance.RemainingLeaves} leave days remaining.";
        //            return RedirectToAction("ApplyLeave");
        //        }
        //    }

        //    // Set model properties
        //    model.UserId = userId;
        //    model.Status = "Pending";
        //    model.ApprovedBy = string.Empty;
        //    model.StatusHistory = string.Empty;
        //    model.NumberOfDays = (model.EndDate - model.StartDate).Days + 1;

        //    db.LeaveRequests.Add(model);
        //    db.SaveChanges();

        //    TempData["success"] = "Your leave request has been sent for approval.";
        //    return RedirectToAction("ApplyLeave");
        //}








        [HttpPost]
        public IActionResult ApplyLeave(LeaveRequest model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Employee")
            {
                TempData["error"] = "You do not have permission to perform this action.";
                return RedirectToAction("ApplyLeave", "Leave");
            }

            var email = User.FindFirstValue(ClaimTypes.Name);
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            if (model.LeaveTypeId == 0)
            {
                TempData["error"] = "Please select a leave type.";
                return RedirectToAction("ApplyLeave");
            }

            var leaveTypeStatus = db.DepartmentLeaves
                .Where(dl => dl.LeaveTypeId == model.LeaveTypeId)
                .Select(dl => dl.Status)
                .FirstOrDefault();

            if (leaveTypeStatus == "Inactive")
            {
                TempData["error"] = "The selected leave type is inactive and cannot be applied for.";
                return RedirectToAction("ApplyLeave");
            }

            var leaveBalance = db.LeaveBalances
                .FirstOrDefault(lb => lb.User.Email == email && lb.LeaveTypeId == model.LeaveTypeId);

            if (leaveBalance == null)
            {
                TempData["error"] = "Leave balance not found for the selected leave type.";
                return RedirectToAction("ApplyLeave");
            }

            if (model.EndDate < model.StartDate)
            {
                TempData["error"] = "End date cannot be earlier than start date.";
                return RedirectToAction("ApplyLeave");
            }

            // Fetch all active holidays between start and end date
            int event_type_id = db.EventTypes.Where(x => x.Name.Trim().Equals("Holiday") || x.Name.Trim().Equals("holiday")).Select(x => x.Id).SingleOrDefault();

            var holidays = db.Events
                .Where(e => e.Status == "Active" && e.EventTypeId == event_type_id)
                .Select(e => e.Date)
                .ToList();


            int numberOfDays = 0;
            for (DateTime date = model.StartDate; date <= model.EndDate; date = date.AddDays(1))
            {
                if (!holidays.Contains(date.ToString("yyyy-MM-dd"))) // Exclude holidays
                {
                    numberOfDays++;
                }
            }

            if (numberOfDays <= 0)
            {
                TempData["error"] = "Invalid leave duration.";
                return RedirectToAction("ApplyLeave");
            }

            if (leaveBalance.RemainingLeaves < numberOfDays)
            {
                TempData["error"] = $"Insufficient leave balance. You only have {leaveBalance.RemainingLeaves} leave days remaining.";
                return RedirectToAction("ApplyLeave");
            }

            // **DO NOT Deduct leave balance here** - It should only be deducted upon approval

            // Set model properties
            model.UserId = userId;
            model.Status = "Pending";
            model.ApprovedBy = string.Empty;
            model.StatusHistory = string.Empty;
            model.NumberOfDays = numberOfDays;

            db.LeaveRequests.Add(model);
            db.SaveChanges();

            TempData["success"] = "Your leave request has been sent for approval.";
            return RedirectToAction("ApplyLeave");
        }


        [HttpPost]
        public IActionResult UpdateLeaveRequest(int LeaveRequestId, string action)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Manager")
            {
                TempData["error"] = "You do not have permission to perform this action.";
                return RedirectToAction("ViewLeaveRequests", "Leave");
            }

            var email = User.FindFirstValue(ClaimTypes.Name);
            var currentUser = db.User.Include(u => u.Role).Include(u => u.Department).FirstOrDefault(u => u.Email == email);

            var leaveRequest = db.LeaveRequests
                .Include(lr => lr.User)
                .FirstOrDefault(lr => lr.LeaveRequestId == LeaveRequestId);

            if (leaveRequest == null)
            {
                TempData["error"] = "Leave request not found.";
                return RedirectToAction("ViewLeaveRequests");
            }

            if (leaveRequest.User.DepartmentId != currentUser.DepartmentId)
            {
                TempData["error"] = "You are only allowed to manage leave requests for employees in your department.";
                return RedirectToAction("ViewLeaveRequests");
            }

            var leaveBalance = db.LeaveBalances
                .FirstOrDefault(lb => lb.UserId == leaveRequest.UserId && lb.LeaveTypeId == leaveRequest.LeaveTypeId);

            if (leaveBalance == null)
            {
                TempData["error"] = "Leave balance not found for the employee.";
                return RedirectToAction("ViewLeaveRequests");
            }

            string statusHistory = leaveRequest.StatusHistory;
            string currentStatus = leaveRequest.Status;

            if (action == "Approve")
            {
                if (currentStatus == "Approved")
                {
                    TempData["error"] = "Leave request has already been approved.";
                    return RedirectToAction("ViewLeaveRequests");
                }

                // Fetch all active holidays between start and end date
                int event_type_id = db.EventTypes.Where(x => x.Name.Trim().Equals("Holiday") || x.Name.Trim().Equals("holiday")).Select(x => x.Id).SingleOrDefault();
                
                var holidays = db.Events
                    .Where(e => e.Status == "Active" && e.EventTypeId == event_type_id)
                    .Select(e => e.Date)
                    .ToList();



                int actualLeaveDays = 0;
                for (DateTime date = leaveRequest.StartDate; date <= leaveRequest.EndDate; date = date.AddDays(1))
                {
                    if (!holidays.Contains(date.ToString("yyyy-MM-dd"))) // Exclude holidays
                    {
                        actualLeaveDays++;
                    }
                }

                if (leaveBalance.RemainingLeaves >= actualLeaveDays)
                {
                    leaveBalance.UsedLeaves += actualLeaveDays;
                }
                else
                {
                    TempData["error"] = "Insufficient leave balance.";
                    return RedirectToAction("ViewLeaveRequests");
                }

                leaveRequest.Status = "Approved";
                leaveRequest.StatusHistory = $"{statusHistory}\nApproved on {DateTime.Now}";
                leaveRequest.ApprovedBy = currentUser.FirstName;

                TempData["success"] = "Leave request approved successfully.";
            }
            else if (action == "Reject")
            {
                if (currentStatus == "Rejected")
                {
                    TempData["error"] = "Leave request has already been rejected.";
                    return RedirectToAction("ViewLeaveRequests");
                }

                leaveRequest.Status = "Rejected";
                leaveRequest.StatusHistory = $"{statusHistory}\nRejected on {DateTime.Now}";
                leaveRequest.ApprovedBy = currentUser.FirstName;

                // Revert leave balance if the request was previously approved
                if (currentStatus == "Approved")
                {
                    leaveBalance.UsedLeaves -= leaveRequest.NumberOfDays;

                    if (leaveBalance.UsedLeaves < 0)
                    {
                        leaveBalance.UsedLeaves = 0;
                    }
                }

                TempData["success"] = "Leave request rejected successfully.";
            }
            else
            {
                TempData["error"] = "Invalid action.";
                return RedirectToAction("ViewLeaveRequests");
            }

            db.SaveChanges();

            return RedirectToAction("ViewLeaveRequests");
        }





        public IActionResult ViewLeaveRequests(string leaveStatus)
        {
            //var userId = GetCurrentUserId();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Manager")
            {
                TempData["error"] = "You do not have permission to perform this action.";
                return RedirectToAction("ViewLeaveRequests", "Leave");

            }

            var email = User.FindFirstValue(ClaimTypes.Name);
            var user = db.User.Include(x => x.Role).SingleOrDefault(x => x.Email == email);

            //var userId = GetCurrentUserId();
            if (User.IsInRole("Manager"))
            {

                //if (userId == null)
                //{
                //    TempData["Message"] = "Unable to identify the current user.";
                //    return RedirectToAction("SignIn", "Auth");
                //}

                var currentUser = db.User.Include(u => u.Role).Include(u => u.Department).FirstOrDefault(u => u.Email == email);

                //if (currentUser == null || currentUser.Role == null || currentUser.Role.RoleName != "Manager")
                //{
                //    TempData["Message"] = "You are not authorized to view or manage leave requests.";
                //    return RedirectToAction("SignIn", "Auth");
                //}

                // Get the department ID of the manager
                var departmentId = currentUser.DepartmentId;

                // Filter leave requests based on status and department
                var leaveRequestsQuery = db.LeaveRequests
                    .Include(lr => lr.MasterLeaveType)
                    .Include(lr => lr.User)
                    .Where(lr => lr.User.DepartmentId == departmentId) // Only show requests from the same department
                    .AsQueryable();

                if (!string.IsNullOrEmpty(leaveStatus))
                {
                    leaveRequestsQuery = leaveRequestsQuery.Where(lr => lr.Status == leaveStatus);
                }

                ViewBag.Lea = new SelectList(db.MasterLeaveTypes, "LeaveTypeId", "LeaveType");
                var leaveRequests = leaveRequestsQuery.ToList();

                return View(leaveRequests);
            }
            else
            {
                return RedirectToAction("SignIn", "Auth");
            }
        }




        public IActionResult ViewMyLeaveRequests()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Employee")
            {
                TempData["error"] = "You do not have permission to perform this action.";
                return RedirectToAction("ApplyLeave", "Leave");

            }
            var email = User.FindFirstValue(ClaimTypes.Name);

            var user = db.User.Include(x => x.Role).SingleOrDefault(x => x.Email == email);

            //var userId = GetCurrentUserId();
            if (User.IsInRole("Employee"))
            {
                //if (!userId.HasValue)
                //{
                //	TempData["Message"] = "Session expired. Please log in again.";
                //	return RedirectToAction("SignIn", "Auth");
                //}

                var leaveBalances = db.LeaveBalances
                    .Where(lb => lb.User.Email == email)
                    .Include(lb => lb.MasterLeaveType)
                    .ToList();

                var model = leaveBalances.Select(lb => new
                {
                    lb.MasterLeaveType.LeaveType,
                    lb.MasterLeaveType.LeaveTypeId,
                    lb.TotalLeaves,
                    lb.RemainingLeaves
                }).ToList();
                if(model!=null)
                {
                    ViewBag.leaves = model;
                }
            }


            //if (!User.Identity.IsAuthenticated)
            //{
            //    return RedirectToAction("SignIn", "Auth");
            //}

            //var email = User.FindFirstValue(ClaimTypes.Name);
            //var user = db.User.Include(x => x.Role).SingleOrDefault(x => x.Email == email);

            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction("SignIn", "Auth");
            }

            // Retrieve leave requests for the logged-in user
            var leaveRequests = db.LeaveRequests
                .Include(lr => lr.MasterLeaveType)
                .Include(lr => lr.User)
                .Where(lr => lr.UserId == user.UserId)
                .ToList();

            return View(leaveRequests);
        }


        //[HttpPost]
        //public IActionResult UpdateLeaveRequest(int LeaveRequestId, string action)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("SignIn", "Auth");
        //    }

        //    var role = User.FindFirstValue(ClaimTypes.Role);

        //    if (role != "Manager")
        //    {
        //        TempData["error"] = "You do not have permission to perform this action.";
        //        return RedirectToAction("ViewLeaveRequests", "Leave");

        //    }

        //    var email = User.FindFirstValue(ClaimTypes.Name);
        //    var user = db.User.Include(x => x.Role).SingleOrDefault(x => x.Email == email);

        //    //var userId = GetCurrentUserId();
        //    if (User.IsInRole("Manager"))
        //    {

        //        //if (userId == null)
        //        //{
        //        //    TempData["Message"] = "Unable to identify the current user.";
        //        //    return RedirectToAction("SignIn", "Auth");
        //        //}

        //        var currentUser = db.User.Include(u => u.Role).Include(u => u.Department).FirstOrDefault(u => u.Email == email);

        //        //if (currentUser == null || currentUser.Role == null || currentUser.Role.RoleName != "Manager")
        //        //{
        //        //    TempData["Message"] = "You are not authorized to approve or reject leave requests.";
        //        //    return RedirectToAction("SignIn", "Auth");
        //        //}

        //        var leaveRequest = db.LeaveRequests
        //            .Include(lr => lr.User)
        //            .FirstOrDefault(lr => lr.LeaveRequestId == LeaveRequestId);

        //        if (leaveRequest == null)
        //        {
        //            TempData["error"] = "Leave request not found.";
        //            return RedirectToAction("ViewLeaveRequests");
        //        }

        //        // Ensure the leave request belongs to an employee in the same department
        //        if (leaveRequest.User.DepartmentId != currentUser.DepartmentId)
        //        {
        //            TempData["error"] = "You are only allowed to manage leave requests for employees in your department.";
        //            return RedirectToAction("ViewLeaveRequests");
        //        }

        //        string statusHistory = leaveRequest.StatusHistory;
        //        string currentStatus = leaveRequest.Status;

        //        if (action == "Approve")
        //        {
        //            if (currentStatus == "Approved")
        //            {
        //                TempData["error"] = "Leave request has already been approved.";
        //                return RedirectToAction("ViewLeaveRequests");
        //            }

        //            leaveRequest.Status = "Approved";
        //            leaveRequest.StatusHistory = $"{statusHistory}\nApproved on {DateTime.Now}";
        //            leaveRequest.ApprovedBy = currentUser.FirstName;
        //            var leaveBalance = db.LeaveBalances
        //                .FirstOrDefault(lb => lb.UserId == leaveRequest.UserId && lb.LeaveTypeId == leaveRequest.LeaveTypeId);

        //            if (leaveBalance != null)
        //            {
        //                leaveBalance.ApplyLeave(leaveRequest.NumberOfDays);
        //            }

        //            TempData["success"] = "Leave request approved successfully.";
        //        }
        //        else if (action == "Reject")
        //        {
        //            //if (currentStatus == "Pending")
        //            //{
        //            //    TempData["error"] = "Leave request cannot be rejected before it has been approved.";
        //            //    return RedirectToAction("ViewLeaveRequests");
        //            //}

        //            if (currentStatus == "Rejected")
        //            {
        //                TempData["error"] = "Leave request has already been Rejected.";
        //                return RedirectToAction("ViewLeaveRequests");
        //            }

        //            leaveRequest.Status = "Rejected";
        //            leaveRequest.StatusHistory = $"{statusHistory}\nRejected on {DateTime.Now}";
        //            leaveRequest.ApprovedBy = currentUser.FirstName;

        //            var leaveBalance = db.LeaveBalances
        //                .FirstOrDefault(lb => lb.UserId == leaveRequest.UserId && lb.LeaveTypeId == leaveRequest.LeaveTypeId);

        //            if (leaveBalance != null)
        //            {
        //                if (currentStatus == "Approved")
        //                {
        //                    leaveBalance.RevertLeave(leaveRequest.NumberOfDays);
        //                }
        //            }

        //            TempData["error"] = "Leave request rejected successfully.";
        //        }
        //        else
        //        {
        //            TempData["error"] = "Invalid action.";
        //            return RedirectToAction("ViewLeaveRequests");
        //        }

        //        db.SaveChanges();

        //        return RedirectToAction("ViewLeaveRequests");
        //    }
        //    else
        //    {
        //        return RedirectToAction("SignIn", "Auth");
        //    }
        //}

        //[HttpPost]
        //public IActionResult UpdateLeaveRequest(int LeaveRequestId, string action)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("SignIn", "Auth");
        //    }

        //    var role = User.FindFirstValue(ClaimTypes.Role);

        //    if (role != "Manager")
        //    {
        //        TempData["error"] = "You do not have permission to perform this action.";
        //        return RedirectToAction("ViewLeaveRequests", "Leave");
        //    }

        //    var email = User.FindFirstValue(ClaimTypes.Name);
        //    var currentUser = db.User.Include(u => u.Role).Include(u => u.Department).FirstOrDefault(u => u.Email == email);

        //    var leaveRequest = db.LeaveRequests
        //        .Include(lr => lr.User)
        //        .FirstOrDefault(lr => lr.LeaveRequestId == LeaveRequestId);

        //    if (leaveRequest == null)
        //    {
        //        TempData["error"] = "Leave request not found.";
        //        return RedirectToAction("ViewLeaveRequests");
        //    }

        //    if (leaveRequest.User.DepartmentId != currentUser.DepartmentId)
        //    {
        //        TempData["error"] = "You are only allowed to manage leave requests for employees in your department.";
        //        return RedirectToAction("ViewLeaveRequests");
        //    }

        //    var leaveBalance = db.LeaveBalances
        //        .FirstOrDefault(lb => lb.UserId == leaveRequest.UserId && lb.LeaveTypeId == leaveRequest.LeaveTypeId);

        //    if (leaveBalance == null)
        //    {
        //        TempData["error"] = "Leave balance not found for the employee.";
        //        return RedirectToAction("ViewLeaveRequests");
        //    }

        //    string statusHistory = leaveRequest.StatusHistory;
        //    string currentStatus = leaveRequest.Status;

        //    if (action == "Approve")
        //    {
        //        if (currentStatus == "Approved")
        //        {
        //            TempData["error"] = "Leave request has already been approved.";
        //            return RedirectToAction("ViewLeaveRequests");
        //        }

        //        leaveRequest.Status = "Approved";
        //        leaveRequest.StatusHistory = $"{statusHistory}\nApproved on {DateTime.Now}";
        //        leaveRequest.ApprovedBy = currentUser.FirstName;

        //        // Deduct leave balance
        //        if (leaveBalance.RemainingLeaves >= leaveRequest.NumberOfDays)
        //        {
        //            leaveBalance.UsedLeaves += leaveRequest.NumberOfDays;
        //        }
        //        else
        //        {
        //            TempData["error"] = "Insufficient leave balance.";
        //            return RedirectToAction("ViewLeaveRequests");
        //        }

        //        TempData["success"] = "Leave request approved successfully.";
        //    }
        //    else if (action == "Reject")
        //    {
        //        if (currentStatus == "Rejected")
        //        {
        //            TempData["error"] = "Leave request has already been rejected.";
        //            return RedirectToAction("ViewLeaveRequests");
        //        }

        //        leaveRequest.Status = "Rejected";
        //        leaveRequest.StatusHistory = $"{statusHistory}\nRejected on {DateTime.Now}";
        //        leaveRequest.ApprovedBy = currentUser.FirstName;

        //        // Revert leave balance if the request was previously approved
        //        if (currentStatus == "Approved")
        //        {
        //            leaveBalance.UsedLeaves -= leaveRequest.NumberOfDays;

        //            if (leaveBalance.UsedLeaves < 0)
        //            {
        //                leaveBalance.UsedLeaves = 0;
        //            }
        //        }

        //        TempData["success"] = "Leave request rejected successfully.";
        //    }
        //    else
        //    {
        //        TempData["error"] = "Invalid action.";
        //        return RedirectToAction("ViewLeaveRequests");
        //    }

        //    db.SaveChanges();

        //    return RedirectToAction("ViewLeaveRequests");
        //}


    }
}
