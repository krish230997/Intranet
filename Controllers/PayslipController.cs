using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Models;
using Pulse360.Data;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using ClosedXML.Excel;

namespace Pulse360.Controllers
{
    public class PayslipController : Controller
    {
		private readonly ApplicationDbContext db;

		public PayslipController(ApplicationDbContext db)
		{
			this.db = db;
		}

		// =========================================================================
		// 1. Helper Class and Method to Calculate Salary Details
		// =========================================================================
		// This helper class is used to hold the computed values.
		private class SalaryCalculationResult
		{
			public decimal TotalHoursWorked { get; set; }
			public decimal TotalWorkingHoursInMonth { get; set; }
			public decimal HourlyRate { get; set; }
			public decimal TotalEarnings { get; set; }
			public decimal TotalDeductions { get; set; }
			public decimal NetSalary { get; set; }
		}

		// This method calculates the salary details for a given user and period.
		// It uses the same logic for both the view and the PDF.
		private SalaryCalculationResult CalculateSalaryDetails(int userId, int month, int year)
		{
			// Retrieve the employee's salary record.
			
			

            var employeeSalary = db.EmployeeSalaries.FirstOrDefault(s => s.UserId == userId);
			if (employeeSalary == null)
				throw new Exception("Salary details not found");

			// Retrieve the user record (needed for department info).
			var user = db.User.FirstOrDefault(u => u.UserId == userId);
			if (user == null)
				throw new Exception("User not found.");

			// Calculate total working hours for the month (assuming 9 hours per day).
			int totalDaysInMonth = DateTime.DaysInMonth(year, month);
			decimal totalWorkingHoursInMonth = totalDaysInMonth * 9m;

			// Calculate the actual hours worked using fallback logic for each day.
			decimal totalHoursWorked = 0;
			for (int day = 1; day <= totalDaysInMonth; day++)
			{
				DateTime currentDay = new DateTime(year, month, day);

				// 1. Check if the day is a holiday.
				// Note: The EventModel.Date is stored as a string; we parse it to DateTime.
				var holidayEvent = db.Set<EventModel>()
					.Where(e => e.Status.Equals("Active"))
					.AsEnumerable() // client-side evaluation for DateTime.Parse
					.FirstOrDefault(e => DateTime.Parse(e.Date).Date == currentDay.Date);
				if (holidayEvent != null)
				{
					// Holiday found: assume no working hours for the day.
					continue;
				}

				// 2. Check for an Attendance record.
				var attendanceRecord = db.Attendance
					.FirstOrDefault(a => a.UserId == userId && a.Date.Date == currentDay.Date);
				if (attendanceRecord != null)
				{
					totalHoursWorked += attendanceRecord.WorkingHours;
					continue;
				}

				// 3. If no attendance record, check for a Timesheet record.
				var timesheetRecord = db.Timesheets
					.FirstOrDefault(t => t.UserId == userId && t.Date.Date == currentDay.Date);
				if (timesheetRecord != null)
				{
					totalHoursWorked += timesheetRecord.WorkHours;
					continue;
				}

				// 4. If no attendance or timesheet, check if an approved LeaveRequest covers this day.
				var leaveRecord = db.LeaveRequests
					.FirstOrDefault(l => l.UserId == userId
									  && l.StartDate.Date <= currentDay.Date
									  && l.EndDate.Date >= currentDay.Date
									  && l.Status.ToLower() == "approved");
				if (leaveRecord != null)
				{
					totalHoursWorked += 9; // Assume a full day (9 hours) for an approved leave.
					continue;
				}

				// 5. Otherwise, assume 0 hours worked for the day.
			}

			// Calculate the hourly rate based on the full month's standard hours.
			decimal hourlyRate = Math.Round(employeeSalary.TotalSalary / totalWorkingHoursInMonth, 2);

			// Retrieve the department-level earnings and deductions.
			var earningsList = db.Earning
				.Include(e => e.EarningType)
				.Where(e => e.DepartmentId == user.DepartmentId)
				.ToList();
			var deductionsList = db.Deduction
				.Include(d => d.DeductionType)
				.Where(d => d.DepartmentId == user.DepartmentId)
				.ToList();

			// Calculate total earnings and deductions based on the actual hours worked.
			decimal totalEarnings = Math.Round(
				earningsList.Sum(e => (totalHoursWorked * e.EarningsPercentage * hourlyRate) / 100), 2);
			decimal totalDeductions = Math.Round(
				deductionsList.Sum(d => (totalHoursWorked * d.DeductionPercentage * hourlyRate) / 100), 2);

			decimal netSalary = Math.Round(totalEarnings - totalDeductions, 2);

			return new SalaryCalculationResult
			{
				TotalHoursWorked = totalHoursWorked,
				TotalWorkingHoursInMonth = totalWorkingHoursInMonth,
				HourlyRate = hourlyRate,
				TotalEarnings = totalEarnings,
				TotalDeductions = totalDeductions,
				NetSalary = netSalary
			};
		}


		// =========================================================================
		// 2. Employee Salary Creation (Unchanged)
		// =========================================================================
		public IActionResult AddEmployeeSalary()
		{
			var users = db.User.ToList();
			var usersWithFullName = users.Select(u => new
			{
				UserId = u.UserId,
				FullName = $"{u.FirstName} {u.LastName}"
			}).ToList();

			ViewBag.Users = new SelectList(usersWithFullName, "UserId", "FullName");
			ViewBag.Earnings = db.Earning.Include(e => e.EarningType).ToList();
			ViewBag.Deductions = db.Deduction.Include(d => d.DeductionType).ToList();

			return View();
		}

		[HttpPost]
		public IActionResult AddEmployeeSalary(EmployeeSalaries e, List<EmployeeEarnings> Earnings, List<EmployeeDeductions> Deductions)
		{
			ViewBag.Users = new SelectList(db.User.ToList(), "UserId", "FirstName");
			ViewBag.Earnings = db.Earning.Include(et => et.EarningType).ToList();
			ViewBag.Deductions = db.Deduction.Include(dt => dt.DeductionType).ToList();

			// Compute total earnings and deductions.
			decimal totalEarnings = Earnings.Sum(er => er.EarningAmount);
			decimal totalDeductions = Deductions.Sum(d => d.DeductionAmount);

			// Calculate net salary.
			decimal netSalary = totalEarnings - totalDeductions;

			// Save employee salary record.
			var employeeSalary = new EmployeeSalaries
			{
				UserId = e.UserId,
				TotalSalary = e.TotalSalary,
				NetSalary = netSalary,
				CreatedDate = DateTime.Now
			};

			db.EmployeeSalaries.Add(employeeSalary);
			db.SaveChanges();

			// Save earnings.
			foreach (var earning in Earnings)
			{
				var employeeEarning = new EmployeeEarnings
				{
					SalaryId = employeeSalary.SalaryId,
					UserId = e.UserId,
					EarningId = earning.EarningId,
					EarningAmount = earning.EarningAmount
				};
				db.EmployeeEarnings.Add(employeeEarning);
			}

			// Save deductions.
			foreach (var deduction in Deductions)
			{
				var employeeDeduction = new EmployeeDeductions
				{
					SalaryId = employeeSalary.SalaryId,
					UserId = e.UserId,
					DeductionId = deduction.DeductionId,
					DeductionAmount = deduction.DeductionAmount
				};
				db.EmployeeDeductions.Add(employeeDeduction);
			}

			db.SaveChanges();

			TempData["success"] = "Salary Added Successfully!!";
			return RedirectToAction("AddEmployeeSalary");
		}

		// =========================================================================
		// 3. Helper to Get Current User ID
		// =========================================================================
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

		// =========================================================================
		// 4. Payslip View Generation (Using the Shared Calculation Logic)
		// =========================================================================
		public IActionResult GeneratePayslip(string month, int year)
		{
			try
			{
				
                int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
                Console.WriteLine($"🔹 Generating payslip for User ID: {userId}, Month: {month}, Year: {year}");

				// Fetch organization details.
				var organization = db.Organization.FirstOrDefault();
				if (organization == null)
				{
					Console.WriteLine("❌ Organization details not found.");
					return NotFound("Organization details not found.");
				}

				// Fetch user data.
				var user = db.User.Include(u => u.Designation).FirstOrDefault(u => u.UserId == userId);
				if (user == null)
				{
					Console.WriteLine("❌ User not found.");
					return NotFound("User not found.");
				}

				// Convert month name to its numeric value.
				int monthNumber = DateTime.ParseExact(month, "MMMM", null).Month;

				// Use the helper to calculate salary details.
				var salaryResult = CalculateSalaryDetails(userId, monthNumber, year);

				Console.WriteLine($"✅ Total Working Hours: {salaryResult.TotalHoursWorked}, Hourly Rate: {salaryResult.HourlyRate}");
				Console.WriteLine($"✅ Total Earnings: {salaryResult.TotalEarnings}, Total Deductions: {salaryResult.TotalDeductions}, Net Salary: {salaryResult.NetSalary}");

				// Pass data to the view.
				ViewBag.Organization = organization;
				ViewBag.User = user;
				ViewBag.Month = month;
				ViewBag.Year = year;
				ViewBag.TotalWorkingHours = salaryResult.TotalHoursWorked;
				ViewBag.TotalHoursInMonth = salaryResult.TotalWorkingHoursInMonth;
				ViewBag.HourlyRate = salaryResult.HourlyRate;
				ViewBag.TotalEarnings = salaryResult.TotalEarnings;
				ViewBag.TotalDeductions = salaryResult.TotalDeductions;
				ViewBag.NetSalary = salaryResult.NetSalary;

				// Also pass department-level earnings and deductions for the tables.
				ViewBag.Earnings = db.Earning.Include(e => e.EarningType)
										.Where(e => e.DepartmentId == user.DepartmentId).ToList();
				ViewBag.Deductions = db.Deduction.Include(d => d.DeductionType)
										.Where(d => d.DepartmentId == user.DepartmentId).ToList();

				// Generate the PDF payslip.
				GeneratePayslipPDF(userId, monthNumber, year);
				return View("PayslipView");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error generating payslip: {ex.Message}");
				return BadRequest($"Error: {ex.Message}");
			}
		}

		// An overload for testing that accepts a userId.
		[Route("Payslip/GeneratePayslip/{userid}")]
		public IActionResult GeneratePayslip(int userid)
		{
			try
			{
				// For testing, use the current month and year.
				DateTime currentDate = DateTime.Now;
				int monthNumber = currentDate.Month;
				string month = currentDate.ToString("MMMM");
				int year = currentDate.Year;

				Console.WriteLine($"🔹 Generating payslip for User ID: {userid}, Month: {month}, Year: {year}");

				// Fetch organization details.
				var organization = db.Organization.FirstOrDefault();
				if (organization == null)
				{
					Console.WriteLine("❌ Organization details not found.");
					return NotFound("Organization details not found.");
				}

				// Fetch user data.
				var user = db.User.Include(u => u.Designation).FirstOrDefault(u => u.UserId == userid);
				if (user == null)
				{
					Console.WriteLine("❌ User not found.");
					return NotFound("User not found.");
				}

				// Calculate salary details.
				var salaryResult = CalculateSalaryDetails(userid, monthNumber, year);

				Console.WriteLine($"✅ Total Working Hours: {salaryResult.TotalHoursWorked}, Hourly Rate: {salaryResult.HourlyRate}");
				Console.WriteLine($"✅ Total Earnings: {salaryResult.TotalEarnings}, Total Deductions: {salaryResult.TotalDeductions}, Net Salary: {salaryResult.NetSalary}");

				// Pass data to the view.
				ViewBag.Organization = organization;
				ViewBag.User = user;
				ViewBag.Month = month;
				ViewBag.Year = year;
				ViewBag.TotalWorkingHours = salaryResult.TotalHoursWorked;
				ViewBag.TotalHoursInMonth = salaryResult.TotalWorkingHoursInMonth;
				ViewBag.HourlyRate = salaryResult.HourlyRate;
				ViewBag.TotalEarnings = salaryResult.TotalEarnings;
				ViewBag.TotalDeductions = salaryResult.TotalDeductions;
				ViewBag.NetSalary = salaryResult.NetSalary;

				ViewBag.Earnings = db.Earning.Include(e => e.EarningType)
										.Where(e => e.DepartmentId == user.DepartmentId).ToList();
				ViewBag.Deductions = db.Deduction.Include(d => d.DeductionType)
										.Where(d => d.DepartmentId == user.DepartmentId).ToList();

				// Generate the PDF payslip.
				GeneratePayslipPDF(userid, monthNumber, year);
				return View("PayslipView");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error generating payslip: {ex.Message}");
				return BadRequest($"Error: {ex.Message}");
			}
		}

		// =========================================================================
		// 5. PDF Generation (Now Using the Shared Calculation Logic)
		// =========================================================================
		// This method now accepts the userId, month, and year so it uses the same calculation.
		public string GeneratePayslipPDF(int userId, int month, int year)
		{
			try
			{
				QuestPDF.Settings.License = LicenseType.Community;
				DateTime periodDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
				string monthName = periodDate.ToString("MMMM");

				// Fetch organization and user details.
				var organization = db.Organization.FirstOrDefault();
				if (organization == null)
					throw new Exception("Organization details not found.");

				var user = db.User.Include(u => u.Designation).FirstOrDefault(u => u.UserId == userId);
				if (user == null)
					throw new Exception("User not found.");

				// Use the helper to get the calculated salary values.
				var salaryResult = CalculateSalaryDetails(userId, month, year);

				// Get department-level earnings and deductions.
				var earningsList = db.Earning.Include(e => e.EarningType)
					.Where(e => e.DepartmentId == user.DepartmentId)
					.ToList();
				var deductionsList = db.Deduction.Include(d => d.DeductionType)
					.Where(d => d.DepartmentId == user.DepartmentId)
					.ToList();

				// Set up the file path for the PDF.
				string fileName = $"Payslip_{user.FirstName}_{monthName}_{year}.pdf";
				string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "payslips", fileName);
				string directoryPath = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);

				// Generate the PDF using QuestPDF.
				Document.Create(container =>
				{
					container.Page(page =>
					{
						page.Size(PageSizes.A4);
						page.Margin(20);
						page.PageColor(Colors.White);

						// Header
						page.Header().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text(organization.OrganizationName).FontSize(18).Bold();
								col.Item().Text(organization.OrganizationAddress).FontSize(10);
							});
							row.RelativeItem().AlignRight().Column(col =>
							{
								col.Item().Text("Earnings Statement").FontSize(16).Bold();
								col.Item().Text($"Period Ending: {periodDate:MM/dd/yyyy}").FontSize(10);
								col.Item().Text($"Pay Date: {periodDate.AddDays(7):MM/dd/yyyy}").FontSize(10);
							});
						});

						// Content
						page.Content().Column(col =>
						{
							col.Spacing(5);
							// Employee Details
							col.Item().Row(row =>
							{
								row.RelativeItem().Text("Exemptions: Federal: 3, State: 2").FontSize(10);
								row.RelativeItem().AlignRight().Text($"{user.FirstName} {user.LastName}").FontSize(12).Bold();
							});
							col.Item().Text(user.Address).FontSize(10);

							// Earnings & Deductions Tables
							col.Item().Row(row =>
							{
								// Earnings Table
								row.RelativeItem().Column(earningsCol =>
								{
									earningsCol.Item().Text("Earnings").FontSize(12).Bold().Underline();
									earningsCol.Item().Table(table =>
									{
										table.ColumnsDefinition(columns =>
										{
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
										});

										table.Header(header =>
										{
											header.Cell().Text("Earnings").Bold();
											header.Cell().Text("Rate").Bold();
											header.Cell().Text("Hours").Bold();
											header.Cell().Text("This Period").Bold();
											header.Cell().Text("Year to Date").Bold();
										});

										foreach (var earning in earningsList)
										{
											// Calculate the individual earning amount.
											decimal earningAmount = Math.Round(
												(salaryResult.TotalHoursWorked * earning.EarningsPercentage * salaryResult.HourlyRate) / 100, 2);
											// Estimate the hours corresponding to the earning.
											decimal earningHours = earningAmount / salaryResult.HourlyRate;

											table.Cell().Text(earning.EarningType.EarningName);
											table.Cell().Text($"{salaryResult.HourlyRate:C}");
											table.Cell().Text($"{earningHours:F2}");
											table.Cell().Text($"{earningAmount:C}");
											table.Cell().Text($"{(earningAmount * 12):C}"); // YTD
										}
									});

									earningsCol.Item().Text($"Gross Pay: {salaryResult.TotalEarnings:C}").FontSize(12).Bold();
								});

								// Deductions Table
								row.RelativeItem().Column(deductionsCol =>
								{
									deductionsCol.Item().Text("Deductions").FontSize(12).Bold().Underline();
									deductionsCol.Item().Table(table =>
									{
										table.ColumnsDefinition(columns =>
										{
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
										});

										table.Header(header =>
										{
											header.Cell().Text("Deductions").Bold();
											header.Cell().Text("This Period").Bold();
											header.Cell().Text("Year to Date").Bold();
										});

										foreach (var deduction in deductionsList)
										{
											decimal deductionAmount = Math.Round(
												(salaryResult.TotalHoursWorked * deduction.DeductionPercentage * salaryResult.HourlyRate) / 100, 2);
											table.Cell().Text(deduction.DeductionType.DeductionsName);
											table.Cell().Text($"{deductionAmount:C}");
											table.Cell().Text($"{(deductionAmount * 12):C}");
										}
									});

									deductionsCol.Item().Text($"Total Deductions: {salaryResult.TotalDeductions:C}").FontSize(12).Bold();
									deductionsCol.Item().Text($"Total Deductions YTD: {(salaryResult.TotalDeductions * 12):C}").FontSize(12).Bold();
								});
							});

							// Net Pay Section
							col.Item().Row(row =>
							{
								row.RelativeItem().Text("Net Pay:").FontSize(14).Bold();
								row.RelativeItem().AlignRight().Text($"{salaryResult.NetSalary:C}").FontSize(14).Bold();
							});

							// Important Notes
							col.Item().BorderBottom(1).PaddingBottom(5);
							col.Item().Text("Important Notes:").FontSize(12).Bold();
							col.Item().Text("Your hourly rate has been adjusted based on total working hours.");
							col.Item().Text("We encourage participation in our upcoming United Way fund drive.");
						});

						// Footer
						page.Footer().Text($"Generated on {DateTime.Now}").FontSize(10).AlignRight();
					});
				}).GeneratePdf(filePath);

				// Save the payslip record.
				var payslip = new Payslips
				{
					UserId = userId,
					Month = monthName,
					Year = year,
					PayslipPath = filePath,
					GeneratedOn = DateTime.Now
				};
				db.Payslips.Add(payslip);
				db.SaveChanges();

				// Send the payslip by email.
				SendPayslipEmail(user.Email, filePath);

				Console.WriteLine($"✅ Payslip saved successfully: {filePath}");
				return filePath;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error generating payslip: {ex.Message}");
				return null;
			}
		}

		// This method works similarly for a user-selected period.
		public string GeneratePayslipPDFForSelectedPeriod(int userId, string month, int year)
		{
			try
			{
				QuestPDF.Settings.License = LicenseType.Community;
				int monthNumber = DateTime.ParseExact(month, "MMMM", null).Month;
				DateTime periodDate = new DateTime(year, monthNumber, DateTime.DaysInMonth(year, monthNumber));

				// Fetch organization and user details.
				var organization = db.Organization.FirstOrDefault();
				if (organization == null)
					throw new Exception("Organization details not found.");

				var user = db.User.Include(u => u.Designation).FirstOrDefault(u => u.UserId == userId);
				if (user == null)
					throw new Exception("User not found.");

				// Calculate salary details.
				var salaryResult = CalculateSalaryDetails(userId, monthNumber, year);

				// Get department-level earnings and deductions.
				var earningsList = db.Earning.Include(e => e.EarningType)
					.Where(e => e.DepartmentId == user.DepartmentId)
					.ToList();
				var deductionsList = db.Deduction.Include(d => d.DeductionType)
					.Where(d => d.DepartmentId == user.DepartmentId)
					.ToList();

				// Set up the file path.
				string fileName = $"Payslip_{user.FirstName}_{month}_{year}.pdf";
				string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "payslips", fileName);
				string directoryPath = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);

				// Generate the PDF.
				Document.Create(container =>
				{
					container.Page(page =>
					{
						page.Size(PageSizes.A4);
						page.Margin(20);
						page.PageColor(Colors.White);

						// Header.
						page.Header().Row(row =>
						{
							row.RelativeItem().Column(col =>
							{
								col.Item().Text(organization.OrganizationName).FontSize(18).Bold();
								col.Item().Text(organization.OrganizationAddress).FontSize(10);
							});
							row.RelativeItem().AlignRight().Column(col =>
							{
								col.Item().Text("Earnings Statement").FontSize(16).Bold();
								col.Item().Text($"Period Ending: {periodDate:MM/dd/yyyy}").FontSize(10);
								col.Item().Text($"Pay Date: {periodDate.AddDays(7):MM/dd/yyyy}").FontSize(10);
							});
						});

						// Content.
						page.Content().Column(col =>
						{
							col.Spacing(5);
							// Employee Details.
							col.Item().Row(row =>
							{
								row.RelativeItem().Text("Exemptions: Federal: 3, State: 2").FontSize(10);
								row.RelativeItem().AlignRight().Text($"{user.FirstName} {user.LastName}").FontSize(12).Bold();
							});
							col.Item().Text(user.Address).FontSize(10);

							// Earnings & Deductions Tables.
							col.Item().Row(row =>
							{
								// Earnings Table.
								row.RelativeItem().Column(earningsCol =>
								{
									earningsCol.Item().Text("Earnings").FontSize(12).Bold().Underline();
									earningsCol.Item().Table(table =>
									{
										table.ColumnsDefinition(columns =>
										{
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
										});

										table.Header(header =>
										{
											header.Cell().Text("Earnings").Bold();
											header.Cell().Text("Rate").Bold();
											header.Cell().Text("Hours").Bold();
											header.Cell().Text("This Period").Bold();
											header.Cell().Text("Year to Date").Bold();
										});

										foreach (var earning in earningsList)
										{
											decimal earningAmount = Math.Round(
												(salaryResult.TotalHoursWorked * earning.EarningsPercentage * salaryResult.HourlyRate) / 100, 2);
											decimal earningHours = earningAmount / salaryResult.HourlyRate;

											table.Cell().Text(earning.EarningType.EarningName);
											table.Cell().Text($"{salaryResult.HourlyRate:C}");
											table.Cell().Text($"{earningHours:F2}");
											table.Cell().Text($"{earningAmount:C}");
											table.Cell().Text($"{(earningAmount * 12):C}");
										}
									});

									earningsCol.Item().Text($"Gross Pay: {salaryResult.TotalEarnings:C}").FontSize(12).Bold();
								});

								// Deductions Table.
								row.RelativeItem().Column(deductionsCol =>
								{
									deductionsCol.Item().Text("Deductions").FontSize(12).Bold().Underline();
									deductionsCol.Item().Table(table =>
									{
										table.ColumnsDefinition(columns =>
										{
											columns.RelativeColumn();
											columns.RelativeColumn();
											columns.RelativeColumn();
										});

										table.Header(header =>
										{
											header.Cell().Text("Deductions").Bold();
											header.Cell().Text("This Period").Bold();
											header.Cell().Text("Year to Date").Bold();
										});

										foreach (var deduction in deductionsList)
										{
											decimal deductionAmount = Math.Round(
												(salaryResult.TotalHoursWorked * deduction.DeductionPercentage * salaryResult.HourlyRate) / 100, 2);
											table.Cell().Text(deduction.DeductionType.DeductionsName);
											table.Cell().Text($"{deductionAmount:C}");
											table.Cell().Text($"{(deductionAmount * 12):C}");
										}
									});

									deductionsCol.Item().Text($"Total Deductions: {salaryResult.TotalDeductions:C}").FontSize(12).Bold();
									deductionsCol.Item().Text($"Total Deductions YTD: {(salaryResult.TotalDeductions * 12):C}").FontSize(12).Bold();
								});
							});

							// Net Pay Section.
							col.Item().Row(row =>
							{
								row.RelativeItem().Text("Net Pay:").FontSize(14).Bold();
								row.RelativeItem().AlignRight().Text($"{salaryResult.NetSalary:C}").FontSize(14).Bold();
							});

							// Important Notes.
							col.Item().BorderBottom(1).PaddingBottom(5);
							col.Item().Text("Important Notes:").FontSize(12).Bold();
							col.Item().Text("Your hourly rate has been adjusted based on total working hours.");
							col.Item().Text("We encourage participation in our upcoming United Way fund drive.");
						});

						// Footer.
						page.Footer().Text($"Generated on {DateTime.Now}").FontSize(10).AlignRight();
					});
				}).GeneratePdf(filePath);

				// Save payslip record.
				var payslip = new Payslips
				{
					UserId = userId,
					Month = month,
					Year = year,
					PayslipPath = filePath,
					GeneratedOn = DateTime.Now
				};
				db.Payslips.Add(payslip);
				db.SaveChanges();

				// Send payslip email.
				SendPayslipEmail(user.Email, filePath);

				Console.WriteLine($"✅ Payslip saved successfully: {filePath}");
				return filePath;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error generating payslip: {ex.Message}");
				return null;
			}
		}

		// =========================================================================
		// 6. Email Sending (Unchanged)
		// =========================================================================
		public void SendPayslipEmail(string userEmail, string filePath)
		{
			try
			{
				DateTime currentDate = DateTime.Now;
				string month = currentDate.ToString("MMMM");

				MailMessage mail = new MailMessage();
				mail.From = new MailAddress("sakshimsawant162@gmail.com");
				mail.To.Add(userEmail);
				mail.Subject = "Your Payslip for the Month";
				mail.Body = "Please find your payslip attached.";
				mail.Attachments.Add(new Attachment(filePath));
				SmtpClient smtp = new SmtpClient("smtp.gmail.com");
				smtp.Port = 587;
				smtp.EnableSsl = true;
				smtp.Credentials = new NetworkCredential("sakshimsawant162@gmail.com", "czscembogqficlkq");
				smtp.Send(mail);

				Console.WriteLine($"✅ Payslip sent to {userEmail}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error sending payslip email: {ex.Message}");
			}
		}

		// =========================================================================
		// 7. Additional Actions (MyPayslips, SelectPayslipMonth, etc.)
		// =========================================================================
		public IActionResult MyPayslips()
		{
			// For testing purposes, using a hard-coded userId. Replace with session/cookie-based logic.
			int? userIdNullable = 3;
			if (!userIdNullable.HasValue)
				return RedirectToAction("Login", "Account");

			int userId = userIdNullable.Value;
			var payslips = db.Payslips
				.Where(p => p.UserId == userId)
				.OrderByDescending(p => p.Year)
				.ThenByDescending(p => p.Month)
				.ToList();

			return View(payslips);
		}

		//public IActionResult SelectPayslipMonth()
		//{
		//          int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
		//          var payslips = db.Payslips
		//		.Where(p => p.UserId == userId)
		//		.OrderByDescending(p => p.Year)
		//		.ThenByDescending(p => p.Month)
		//		.ToList();

		//	return View(payslips);
		//}

		public IActionResult SelectPayslipMonth(string? month, int? year)
		{
			int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

			var payslips = db.Payslips
				.Where(p => p.UserId == userId)
				.AsQueryable();

			// Apply Month Filter
			if (!string.IsNullOrEmpty(month))
				payslips = payslips.Where(p => p.Month == month);

			// Apply Year Filter
			if (year.HasValue)
				payslips = payslips.Where(p => p.Year == year.Value);

			ViewBag.AvailableYears = Enumerable.Range(DateTime.Now.Year - 10, 11).Reverse().ToList();
			return View(payslips.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList());
		}


		[HttpPost]
		public IActionResult GeneratePayslipsForAllUsers()
		{
			try
			{
				var users = db.User.ToList();
				foreach (var user in users)
				{
					try
					{
						// Ensure salary data exists.
						if (!db.EmployeeSalaries.Any(s => s.UserId == user.UserId))
						{
							Console.WriteLine($"⚠️ Salary details missing for User ID: {user.UserId}. Skipping.");
							continue;
						}

						// Generate PDF for the current period.
						DateTime currentDate = DateTime.Now;
						GeneratePayslipPDF(user.UserId, currentDate.Month, currentDate.Year);
					}
					catch (Exception exUser)
					{
						Console.WriteLine($"❌ Error generating payslip for User ID {user.UserId}: {exUser.Message}");
					}
				}
				return Ok("Payslips have been generated and emailed for all users.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error generating payslips for all users: {ex.Message}");
				return BadRequest($"Error: {ex.Message}");
			}
		}

        public IActionResult GeneratePayslipsForAllUsersByDate()
        {
            // This view will contain a form to select the month and year.
            var users = db.User.ToList();
            var usersWithFullName = users.Select(u => new
            {
                UserId = u.UserId,
                FullName = $"{u.FirstName} {u.LastName}"
            }).ToList();

            ViewBag.Users = new SelectList(usersWithFullName, "UserId", "FullName");

            return View();
        }

        [HttpPost]
        public IActionResult GeneratePayslipsForAllUsersByDate(string Month, int Year, int UserId)
        {
            if (string.IsNullOrEmpty(Month) || Year < 2000 || Year > 2100 || UserId <= 0)
            {
                TempData["AlertMessage"] = "Invalid input. Please check the selected user, month, and year.";
                return RedirectToAction("GeneratePayslipsForAllUsersByDate");
            }

            try
            {
                // Convert month to numeric representation
                int monthNumber = DateTime.ParseExact(Month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;

                Console.WriteLine($"🔹 Generating payslip for User ID: {UserId}, Month: {Month}, Year: {Year}");

                var organization = db.Organization.FirstOrDefault();
                if (organization == null)
                {
                    TempData["AlertMessage"] = "Organization details not found.";
                    return RedirectToAction("GeneratePayslipsForAllUsersByDate");
                }

                var user = db.User.Include(u => u.Designation).FirstOrDefault(u => u.UserId == UserId);
                if (user == null)
                {
                    TempData["AlertMessage"] = "User not found.";
                    return RedirectToAction("GeneratePayslipsForAllUsersByDate");
                }

                var employeeSalary = db.EmployeeSalaries.FirstOrDefault(s => s.UserId == UserId);
                if (employeeSalary == null)
                {
                    TempData["AlertMessage"] = "Salary details not found.";
                    return RedirectToAction("GeneratePayslipsForAllUsersByDate");
                }

                // Calculate salary details
                var salaryResult = CalculateSalaryDetails(UserId, monthNumber, Year);

                // Store data in ViewBag for the view
                ViewBag.Organization = organization;
                ViewBag.User = user;
                ViewBag.Month = Month;
                ViewBag.Year = Year;
                ViewBag.TotalWorkingHours = salaryResult.TotalHoursWorked;
                ViewBag.HourlyRate = salaryResult.HourlyRate;
                ViewBag.TotalEarnings = salaryResult.TotalEarnings;
                ViewBag.TotalDeductions = salaryResult.TotalDeductions;
                ViewBag.NetSalary = salaryResult.NetSalary;

                ViewBag.Earnings = db.Earning.Include(e => e.EarningType)
                                        .Where(e => e.DepartmentId == user.DepartmentId).ToList();
                ViewBag.Deductions = db.Deduction.Include(d => d.DeductionType)
                                        .Where(d => d.DepartmentId == user.DepartmentId).ToList();

                // Generate PDF payslip
                GeneratePayslipPDF(UserId, monthNumber, Year);

                TempData["success"] = "Payslip generated successfully!";
                return View("PayslipView"); // Redirect to main payslip listing
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating payslip: {ex.Message}");
                TempData["AlertMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("GeneratePayslipsForAllUsersByDate");
            }
        }

        public ActionResult DownloadPayslip(string payslipPath)
        {
            string fileName = Path.GetFileName(payslipPath);
            // Return the file as a download
            byte[] fileBytes = System.IO.File.ReadAllBytes(payslipPath);
            return File(fileBytes, "application/pdf", fileName);
         
        }

		public IActionResult DeleteTransaction(int payslipId)
		{
			var data=db.Payslips.Find(payslipId);
			if(data!=null)
			{
                db.Payslips.RemoveRange(data);
                db.SaveChanges();
				TempData["error"] = "Transaction Deleted Successfully!!";
                return RedirectToAction("TransactionHistory");
            }
			else
			{
				return null;
			}
			
		}


        public IActionResult TransactionHistory(string? month, int? year, int? department, int? designation)
		{
			ViewBag.Departments = db.Departments.ToList();
			ViewBag.Designations = db.Designations.ToList();

			var payslips = db.Payslips
				.Include(p => p.User)
				.ThenInclude(u => u.Department)
				.Include(p => p.User.Designation)
				.AsQueryable();

			// Filtering by month (Stored as "January", "February", etc.)
			if (!string.IsNullOrEmpty(month))
				payslips = payslips.Where(p => p.Month == month);

			// Filtering by year (Stored as an integer)
			if (year.HasValue)
				payslips = payslips.Where(p => p.Year == year.Value);

			// Filtering by department (Using Department ID)
			if (department.HasValue)
				payslips = payslips.Where(p => p.User.DepartmentId == department.Value);

			// Filtering by designation (Using Designation ID)
			if (designation.HasValue)
				payslips = payslips.Where(p => p.User.DesignationtId == designation.Value);

			return View(payslips.OrderByDescending(p => p.GeneratedOn).ToList());
		}


		

		public IActionResult GeneratePayslipforAllUser(int userId, string month, int year)
		{
			try
			{
				Console.WriteLine($"🔹 Generating payslip for User ID: {userId}, Month: {month}, Year: {year}");

				// *Fetch Organization Details*
				var organization = db.Organization.FirstOrDefault();
				if (organization == null)
				{
                    //return NotFound("Organization details not found.");
                    TempData["msg"] = "Organization details not found";
                }

				// *Fetch User Data*
				var user = db.User.Include(u => u.Designation).FirstOrDefault(u => u.UserId == userId);
				if (user == null)
				{
					//return NotFound("User not found.");
                    TempData["msg"] = "User not found";
                }

				// *Fetch Employee Salary*
				var employeeSalary = db.EmployeeSalaries.FirstOrDefault(s => s.UserId == userId);
				if (employeeSalary == null)
				{
					//return NotFound("Salary details not found.");
					TempData["msg"] = "Salary Details not Found";
				}

				// *Calculate total working hours for the selected month*
				int totalDaysInMonth = DateTime.DaysInMonth(year, DateTime.ParseExact(month, "MMMM", null).Month);
				decimal totalWorkingHoursInMonth = totalDaysInMonth * 9m;

				// *Calculate Hourly Rate*
				decimal hourlyRate = Math.Round(employeeSalary.TotalSalary / totalWorkingHoursInMonth, 2);

				// *Fetch Attendance Data*
				var attendanceRecords = db.Attendance
					.Where(a => a.UserId == userId && a.Date.Month == DateTime.ParseExact(month, "MMMM", null).Month && a.Date.Year == year)
					.ToList();

				// *Calculate Total Hours Worked*
				decimal totalHoursWorked = attendanceRecords.Sum(a => a.WorkingHours);

				// *Fetch Earnings & Deductions*
				var earnings = db.Earning.Include(e => e.EarningType).Where(e => e.DepartmentId == user.DepartmentId).ToList();
				var deductions = db.Deduction.Include(d => d.DeductionType).Where(d => d.DepartmentId == user.DepartmentId).ToList();

				// *Calculate Total Earnings & Deductions*
				decimal totalEarnings = Math.Round(earnings.Sum(e => (totalHoursWorked * e.EarningsPercentage * hourlyRate) / 100), 2);
				decimal totalDeductions = Math.Round(deductions.Sum(d => (totalHoursWorked * d.DeductionPercentage * hourlyRate) / 100), 2);
				decimal netSalary = Math.Round(totalEarnings - totalDeductions, 2);

				// Pass Data to View
				ViewBag.Organization = organization;
				ViewBag.User = user;
				ViewBag.Month = month;
				ViewBag.Year = year;
				ViewBag.TotalWorkingHours = totalHoursWorked;
				ViewBag.TotalHoursInMonth = totalWorkingHoursInMonth;
				ViewBag.HourlyRate = hourlyRate;
				ViewBag.TotalEarnings = totalEarnings;
				ViewBag.TotalDeductions = totalDeductions;
				ViewBag.NetSalary = netSalary;
				ViewBag.Earnings = earnings;
				ViewBag.Deductions = deductions;

				return View("PayslipView"); // Show the Payslip View
			}
			catch (Exception ex)
			{
				return BadRequest($"Error: {ex.Message}");
			}
		}

		public IActionResult ExportToPdf()
		{
			QuestPDF.Settings.License = LicenseType.Community;

			var payslips = db.Payslips
				.Include(p => p.User)
				.Include(p => p.User.Department)
				.Include(p => p.User.Designation)
				.ToList();

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(20);
					page.Header()
						.Text("Payslip Report")
						.SemiBold().FontSize(16).AlignCenter();

					page.Content()
						.Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.ConstantColumn(50);  // Emp ID
								columns.RelativeColumn(2);  // Employee
								columns.RelativeColumn(2);  // Email
								columns.RelativeColumn(2);  // Phone
								columns.RelativeColumn(2);  // Department
								columns.RelativeColumn(1);  // Generated On
								columns.RelativeColumn(1);  // Month/Year
							});

							table.Header(header =>
							{
								header.Cell().Text("Emp ID").Bold();
								header.Cell().Text("Employee").Bold();
								header.Cell().Text("Email").Bold();
								header.Cell().Text("Phone").Bold();
								header.Cell().Text("Department").Bold();
								header.Cell().Text("Generated On").Bold();
								header.Cell().Text("Month / Year").Bold();
							});

							foreach (var payslip in payslips)
							{
								table.Cell().Text(payslip.User.UserId.ToString());
								table.Cell().Text($"{payslip.User.FirstName} {payslip.User.LastName}");
								table.Cell().Text(payslip.User.Email);
								table.Cell().Text(payslip.User.PhoneNumber);
								table.Cell().Text(payslip.User.Department.Name);
								table.Cell().Text(payslip.GeneratedOn.ToString("dd/MM/yyyy"));
								table.Cell().Text($"{payslip.Month} / {payslip.Year}");
							}
						});
				});
			});

			byte[] pdfBytes = document.GeneratePdf();
			return File(pdfBytes, "application/pdf", "Payslips.pdf");
		}

		public IActionResult ExportToExcel()
		{
			var payslips = db.Payslips
				.Include(p => p.User)
				.Include(p => p.User.Department)
				.Include(p => p.User.Designation)
				.ToList();

			using (var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Payslips");

				// Add headers
				worksheet.Cell(1, 1).Value = "Emp ID";
				worksheet.Cell(1, 2).Value = "Employee";
				worksheet.Cell(1, 3).Value = "Email";
				worksheet.Cell(1, 4).Value = "Phone";
				worksheet.Cell(1, 5).Value = "Department";
				worksheet.Cell(1, 6).Value = "Generated On";
				worksheet.Cell(1, 7).Value = "Month / Year";

				int row = 2; // Start from row 2 since row 1 has headers
				foreach (var payslip in payslips)
				{
					worksheet.Cell(row, 1).Value = payslip.User.UserId;
					worksheet.Cell(row, 2).Value = $"{payslip.User.FirstName} {payslip.User.LastName}";
					worksheet.Cell(row, 3).Value = payslip.User.Email;
					worksheet.Cell(row, 4).Value = payslip.User.PhoneNumber;
					worksheet.Cell(row, 5).Value = payslip.User.Department.Name;
					worksheet.Cell(row, 6).Value = payslip.GeneratedOn.ToString("dd/MM/yyyy");
					worksheet.Cell(row, 7).Value = $"{payslip.Month} / {payslip.Year}";
					row++;
				}

				using (var stream = new MemoryStream())
				{
					workbook.SaveAs(stream);
					var content = stream.ToArray();
					return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Payslips.xlsx");
				}
			}
		}
	}
}
