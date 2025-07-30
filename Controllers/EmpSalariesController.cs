using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;
using Pulse360.Models;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

using System.Net.Mail;
using System.Net;

namespace Pulse360.Controllers
{
	public class EmpSalariesController : Controller
	{
		private readonly ApplicationDbContext db;
		public EmpSalariesController(ApplicationDbContext db)
		{
			this.db = db;
		}

		public async Task<IActionResult> EmployeeSalary(string designation, string dateRange, string sortBy)
		{
			var query = db.EmployeeSalaries
				.Include(e => e.User)
				.ThenInclude(u => u.Designation)
				.AsQueryable();

			query = query.Where(e => e.User.Role.RoleName != "Admin");

			// Filter by Designation
			if (!string.IsNullOrEmpty(designation))
			{
				query = query.Where(e => e.User.Designation.Name == designation);
			}

			// Filter by Date Range
			if (!string.IsNullOrEmpty(dateRange))
			{
				var dates = dateRange.Split(" - ");
				if (dates.Length == 2 && DateTime.TryParse(dates[0], out var startDate) && DateTime.TryParse(dates[1], out var endDate))
				{
					query = query.Where(e => e.User.DateOfJoining >= startDate && e.User.DateOfJoining <= endDate);
				}
			}

			// Sorting
			switch (sortBy)
			{
				case "Recently Added":
					query = query.OrderByDescending(e => e.User.DateOfJoining);
					break;
				case "Ascending":
					query = query.OrderBy(e => e.User.FirstName);
					break;
				case "Descending":
					query = query.OrderByDescending(e => e.User.FirstName);
					break;
				case "Last Month":
					query = query.Where(e => e.User.DateOfJoining >= DateTime.Now.AddMonths(-1));
					break;
				case "Last 7 Days":
					query = query.Where(e => e.User.DateOfJoining >= DateTime.Now.AddDays(-7));
					break;
			}

			var employeeSalaries = await query.ToListAsync();
			return View(employeeSalaries);
		}

		// Delete Employee Salary (GET)
		public async Task<IActionResult> Delete(int id)
		{
			var salary = await db.EmployeeSalaries
				.Include(e => e.User)
				.ThenInclude(u => u.Designation)
				.FirstOrDefaultAsync(e => e.SalaryId == id);

			if (salary == null)
			{
				return NotFound();
			}

			return View(salary);
		}

		// Delete Employee Salary (POST)
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var salary = await db.EmployeeSalaries.FindAsync(id);
			if (salary != null)
			{
				db.EmployeeSalaries.Remove(salary);
				await db.SaveChangesAsync();
			}

			return RedirectToAction("EmployeeSalary");
		}

        //[HttpPost]
        //public IActionResult GeneratePayslipsForSelectedUsers([FromBody] List<int> employeeIds)
        //{
        //	try
        //	{
        //		if (employeeIds == null || !employeeIds.Any())
        //		{
        //			return BadRequest("No employee IDs provided.");
        //		}

        //		Console.WriteLine($"✅ Received Employee IDs: {string.Join(", ", employeeIds)}"); // Debugging

        //		foreach (var userId in employeeIds)
        //		{
        //			try
        //			{
        //				var salaryExists = db.EmployeeSalaries.Any(s => s.UserId == userId);
        //				if (!salaryExists)
        //				{
        //					Console.WriteLine($"⚠ Salary details missing for User ID: {userId}. Skipping payslip generation.");
        //					continue;
        //				}

        //				GeneratePayslipPDF(userId);
        //			}
        //			catch (Exception exUser)
        //			{
        //				Console.WriteLine($"❌ Error generating payslip for User ID {userId}: {exUser.Message}");
        //			}
        //		}

        //		return Ok("Payslips have been generated and emailed for selected users.");
        //	}
        //	catch (Exception ex)
        //	{
        //		Console.WriteLine($"❌ Error generating payslips for selected users: {ex.Message}");
        //		return BadRequest($"Error: {ex.Message}");
        //	}
        //}

        [HttpPost]
        public IActionResult GeneratePayslipsForSelectedUsers([FromBody] List<int> employeeIds)
        {
            try
            {
                if (employeeIds == null || !employeeIds.Any())
                {
                    return BadRequest(new { message = "No employee IDs provided." });
                }

                Console.WriteLine($"✅ Received Employee IDs: {string.Join(", ", employeeIds)}"); // Debugging

                List<int> failedIds = new List<int>(); // Track failed employees

                foreach (var userId in employeeIds)
                {
                    try
                    {
                        var salaryExists = db.EmployeeSalaries.Any(s => s.UserId == userId);
                        if (!salaryExists)
                        {
                            Console.WriteLine($"⚠ Salary details missing for User ID: {userId}. Skipping payslip generation.");
                            failedIds.Add(userId);
                            continue;
                        }

                        string payslipPath = GeneratePayslipPDF(userId);
                        if (string.IsNullOrEmpty(payslipPath))
                        {
                            Console.WriteLine($"❌ Failed to generate payslip for User ID: {userId}");
                            failedIds.Add(userId);
                        }
                    }
                    catch (Exception exUser)
                    {
                        Console.WriteLine($"❌ Error generating payslip for User ID {userId}: {exUser.Message}");
                        failedIds.Add(userId);
                    }
                }

                if (failedIds.Any())
                {
                    return Ok(new
                    {
                        message = "Payslips generated with some failures.",
                        failedUsers = failedIds
                    });
                }

                return Ok(new { message = "Payslips have been generated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating payslips for selected users: {ex.Message}");
                return StatusCode(500, new { message = $"Server Error: {ex.Message}" });
            }
        }


        public string GeneratePayslipPDF(int userId)
		{
			try
			{
				QuestPDF.Settings.License = LicenseType.Community;

				DateTime currentDate = DateTime.Now;
				string month = currentDate.ToString("MMMM");
				int year = currentDate.Year;

				// Fetch organization details
				var organization = db.Organization.FirstOrDefault();
				if (organization == null) throw new Exception("Organization details not found.");

				// Fetch user details
				var user = db.User.Include(u => u.Designation).FirstOrDefault(u => u.UserId == userId);
				if (user == null) throw new Exception("User not found.");

				// Fetch employee salary details
				var employeeSalary = db.EmployeeSalaries.FirstOrDefault(s => s.UserId == userId);
				if (employeeSalary == null) throw new Exception("Salary details not found.");

				// Fetch earnings (ENSURE NO DUPLICATES)
				var earnings = db.EmployeeEarnings
				 .Include(e => e.Earning)   // Include Earning
				 .ThenInclude(e => e.EarningType)  // Include EarningType inside Earning
				 .Where(e => e.UserId == userId)
				 .GroupBy(e => e.Earning.EarningType.EarningName)  // NullReference happens here
				 .Select(g => g.First())
				 .ToList();


				// Fetch deductions (ENSURE NO DUPLICATES)
				var deductions = db.EmployeeDeductions
				   .Include(d => d.Deduction)  // Include Deduction
				   .ThenInclude(d => d.DeductionType)  // Include DeductionType inside Deduction
				   .Where(d => d.UserId == userId)
				   .GroupBy(d => d.Deduction.DeductionType.DeductionsName)  // Ensure DeductionType is not null
				   .Select(g => g.First())
				   .ToList();


				// Calculate Total Earnings and Total Deductions
				decimal totalEarnings = earnings.Sum(e => e.EarningAmount);
				decimal totalDeductions = deductions.Sum(d => d.DeductionAmount);
				decimal netSalary = totalEarnings - totalDeductions;

				// Calculate Year-To-Date (YTD) Values (Multiply Monthly Earnings/Deductions by 12)
				decimal totalEarningsYTD = totalEarnings * 12;
				decimal totalDeductionsYTD = totalDeductions * 12;

				// Calculate Working Hours Dynamically
				decimal calculatedHours = earnings.Count > 0 ? earnings.Average(e => e.EarningAmount / 39.68m) : 0m; // Assuming rate is 39.68

				// File path setup
				string fileName = $"Payslip_{user.FirstName}_{month}_{year}.pdf";
				string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "payslips", fileName);

				// Ensure directory exists
				string directoryPath = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				// Generate PDF using QuestPDF
				Document.Create(container =>
				{
					container.Page(page =>
					{
						page.Size(PageSizes.A4);
						page.Margin(20);
						page.PageColor(Colors.White);

						// Header Section
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
								col.Item().Text($"Period Ending: {currentDate:MM/dd/yyyy}").FontSize(10);
								col.Item().Text($"Pay Date: {currentDate.AddDays(7):MM/dd/yyyy}").FontSize(10);
							});
						});

						page.Content().Column(col =>
						{
							col.Spacing(5);

							// Employee Details
							col.Item().Row(row =>
							{
								row.RelativeItem().Text($"Exemptions: Federal: 3, State: 2").FontSize(10);
								row.RelativeItem().AlignRight().Text($"{user.FirstName} {user.LastName}").FontSize(12).Bold();
							});

							col.Item().Text(user.Address).FontSize(10);

							// Earnings & Deductions Table
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

										foreach (var earning in earnings)
										{
											decimal earningHours = earning.EarningAmount / 39.68m;
											table.Cell().Text(earning.Earning.EarningType.EarningName);
											table.Cell().Text("$39.68"); // Assuming fixed rate
											table.Cell().Text($"{earningHours:F2}");
											table.Cell().Text($"${earning.EarningAmount:F1}");
											table.Cell().Text($"${(earning.EarningAmount * 12):F1}"); // YTD Calculation
										}
									});

									earningsCol.Item().Text($"Gross Pay: ${totalEarnings:F2}").FontSize(12).Bold();

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

										foreach (var deduction in deductions)
										{
											if (deduction.Deduction != null && deduction.Deduction.DeductionType != null)
											{
												table.Cell().Text(deduction.Deduction.DeductionType.DeductionsName);
												table.Cell().Text($"${deduction.DeductionAmount:F1}");
												table.Cell().Text($"${(deduction.DeductionAmount * 12):F1}"); // YTD Calculation
											}
											else
											{
												// Handle the case where Deduction or DeductionType is null
												table.Cell().Text("No Deduction Name");
												table.Cell().Text("$0.00");
												table.Cell().Text("$0.00");
											}
										}

									});

									deductionsCol.Item().Text($"Total Deductions: ${totalDeductions:F2}").FontSize(12).Bold();
									deductionsCol.Item().Text($"Total Deductions YTD: ${totalDeductionsYTD:F2}").FontSize(12).Bold();
								});
							});

							// Net Pay Section
							col.Item().Row(row =>
							{
								row.RelativeItem().Text("Net Pay:").FontSize(14).Bold();
								row.RelativeItem().AlignRight().Text($"${netSalary:F2}").FontSize(14).Bold();
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

				// Save Payslip Record
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

				// Send Payslip Email
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

		public void SendPayslipEmail(string userEmail, string filePath)
		{
			try
			{
				DateTime currentDate = DateTime.Now;
				string month = currentDate.ToString("MMMM");

				MailMessage mail = new MailMessage();
				mail.From = new MailAddress("sakshimsawant162@gmail.com");
				mail.To.Add(userEmail);
				mail.Subject = "Your Payslip for the Month " + month;
				mail.Body = $"Please find your payslip attached for this month. {month}";
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
	}
}
