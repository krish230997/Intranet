using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pulse360.Controllers;
using Pulse360.Data;
  
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



builder.Services.AddDbContext<ApplicationDbContext>
	(
		options => options.UseSqlServer
		(
			builder.Configuration.GetConnectionString("dbconn")
		)
	);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.ExpireTimeSpan = TimeSpan.FromHours(3);
        option.SlidingExpiration = true; // Use sliding expiration to extend cookie lifetime
        option.LoginPath = "/Auth/SignIn";
        option.AccessDeniedPath = "/Auth/SignIn";
        option.LogoutPath = "/Auth/Logout"; // Logout page
    })
    .AddGoogle(options =>
    {
        var googleAuth = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuth["ClientId"];
        options.ClientSecret = googleAuth["ClientSecret"];
        options.CallbackPath = googleAuth["RedirectUri"].Replace("https://localhost:7110", ""); // Extract path only
    });





builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromHours(1);  // Align with the cookie expiration
	options.Cookie.IsEssential = true;
	options.Cookie.HttpOnly = true;
});



builder.Services.AddAuthorization();


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var termcontroller = new TerminationController(db);
    var procontroller = new PromotionController(db);
    var rescontroller = new ResignationController(db);

    //termcontroller.ChangeStatus();
    //procontroller.ChangeDesignation();
    //rescontroller.ChangeStatus();
}

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();  // Session middleware should come before authentication
app.UseAuthentication(); // Ensure this is added after session
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=SignIn}/{id?}");

app.Run();
