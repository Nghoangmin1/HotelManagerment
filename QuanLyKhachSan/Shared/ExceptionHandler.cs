using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.Shared
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public ExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log exception
                Logger.LogError("An unhandled exception occurred during the request lifecycle.", ex);

                // Handle the exception response
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // If the request path is under Admin area, redirect to Admin error page, otherwise standard error
            var path = context.Request.Path.Value ?? "";
            if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/Admin/Error");
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }

            return Task.CompletedTask;
        }
    }
}
