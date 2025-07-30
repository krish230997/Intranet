using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Pulse360.Controllers
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            string message = "An unexpected error occurred. Please try again.";

            // Check for DbUpdateException and handle specific cases (like foreign key violations)
            if (exception is DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException;

                if (innerException is SqlException sqlEx)
                {
                    string innerMessage = sqlEx.Message.ToLower(); // Normalize for matching

                    // Handle foreign key constraint errors specifically
                    if (innerMessage.Contains("foreign key") ||
                        innerMessage.Contains("constraint fails") ||
                        innerMessage.Contains("conflicted with the foreign key constraint") ||
                        innerMessage.Contains("the delete statement conflicted with the reference constraint"))
                    {
                        message = "Cannot delete this record because it is referenced in other records.";
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // Set BadRequest for constraint issues
                    }
                }
            }

            // Log for debugging purposes
            Console.WriteLine($"Exception: {exception.Message}");
            if (exception.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {exception.InnerException.Message}");
            }

            var response = new { success = false, message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
