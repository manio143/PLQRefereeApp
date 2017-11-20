using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PLQRefereeApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.Name = "Session";
                options.Cookie.HttpOnly = true;
                if (Configuration.GetValue("SessionCookieAlwaysSecure", true))
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                else
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
            services.AddAuthentication(Authentication.Scheme)
                    .AddCookie(options =>
                    {
                        options.AccessDeniedPath = "/forbidden";
                        options.LoginPath = "/login";
                        options.LogoutPath = "/logout";
                        options.ReturnUrlParameter = "returnUrl";
                        options.ExpireTimeSpan = TimeSpan.FromDays(1);
                        options.Cookie.Name = "AuthSession";
                        options.Cookie.SameSite = SameSiteMode.Strict;
                        if (Configuration.GetValue("SessionCookieAlwaysSecure", true))
                            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        else
                            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                        options.Validate();
                    });
            services.AddDbContext<MainContext>(options => options.UseMySql(Configuration.GetConnectionString("Database")));
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "CsrfToken";
                options.Cookie.HttpOnly = false;
                options.FormFieldName = "csrftoken";
                options.HeaderName = "X-CSRF-TOKEN";
                options.SuppressXFrameOptionsHeader = false;
                if (Configuration.GetValue("SessionCookieAlwaysSecure", true))
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                else
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            services.AddTransient<UserRepository, UserRepository>();
            services.AddTransient<TestRepository, TestRepository>();
            services.AddTransient<QuestionRepository, QuestionRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseSession();

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
