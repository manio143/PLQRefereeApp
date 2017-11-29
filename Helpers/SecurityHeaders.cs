using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace PLQRefereeApp
{
    public static class SecurityMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }

    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            IHeaderDictionary headers = context.Response.Headers;

            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            headers["Content-Security-Policy"] = "default-src https: 'self' 'unsafe-inline'";
            headers["X-Frame-Options"] = "sameorigin";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "origin-when-cross-origin";

            await _next(context);
        }
    }
}