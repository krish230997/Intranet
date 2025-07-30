using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Pulse360.Controllers
{
	public class LoginController : Controller
	{
		public IActionResult Login()
		{
			return View();
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
				});

			ViewBag.Name = claims?.FirstOrDefault(x => x.Type.Contains("name"))?.Value;
			ViewBag.Email = claims?.FirstOrDefault(x => x.Type.Contains("email"))?.Value;

			return View("GoogleResponse");
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Login");
		}
	
}
}
