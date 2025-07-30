using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse360.Data;

namespace Pulse360.Controllers
{
	public class AjaxController : Controller
	{
		private readonly ApplicationDbContext db;
		public AjaxController(ApplicationDbContext db)
		{
			this.db = db;
		}
		[HttpGet]
		public IActionResult GetEmployees(int page = 1, string status = "", string department = "", string search = "")
		{
			int pageSize = 10; // Number of items per page
			var query = db.User
				.Include(u => u.Department) // Include related Department
				.AsQueryable();

			// Apply filters
			if (!string.IsNullOrEmpty(status))
				query = query.Where(u => u.Status == status);

			if (!string.IsNullOrEmpty(department))
				query = query.Where(u => u.DepartmentId.ToString() == department);

			if (!string.IsNullOrEmpty(search))
				query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));

			// Pagination
			var totalRecords = query.Count();
			var users = query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(u => new
				{
					u.UserId,
					Name = u.FirstName + " " + u.LastName,
					u.Email,
					DepartmentName = u.Department.Name,
					u.PhoneNumber,
					DateOfJoining = u.DateOfJoining.ToString("yyyy-MM-dd"),
					u.Status
				})
				.ToList();

			return Json(new
			{
				data = users,
				totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
			});
		}
	}
}
