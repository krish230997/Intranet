using Microsoft.AspNetCore.Mvc;

namespace Pulse360.Controllers
{
	public class HRDController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult HRD()
		{
			return View();
		}
	}
}
