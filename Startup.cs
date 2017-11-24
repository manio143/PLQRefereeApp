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
using Microsoft.AspNetCore.Antiforgery;

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
                        options.SlidingExpiration = true;
                        options.Events = new CookieAuthenticationEvents()
                        {
                            OnValidatePrincipal = Authentication.ValidatePrincipal
                        };
                        options.Validate();
                    });
            services.AddDbContext<MainContext>(options => options.UseMySql(Configuration.GetConnectionString("Database")));
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "CSRFToken";
                options.Cookie.HttpOnly = false;
                options.FormFieldName = "csrftoken";
                options.HeaderName = "X-CSRF-Token";
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAntiforgery antiforgery)
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

            //CSRF
            app.Use(next => context =>
            {
                var path = context.Request.Path.Value;
                if (path == "/test" || path == "/test-answer" || path == "/test-start" || path == "/test-finish")
                {
                    var tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append(tokens.HeaderName, tokens.RequestToken,
                            new CookieOptions() { HttpOnly = false });
                }
                return next(context);
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            RunTestSaverAsync();
        }

        public async void RunTestSaverAsync()
        {
            var ctxOptions = new DbContextOptionsBuilder<MainContext>();
            ctxOptions.UseMySql(Configuration.GetConnectionString("Database"));
            while (true)
            {
                await Task.Delay(30 * 1000);
                using (var ctx = new MainContext(ctxOptions.Options))
                {
                    var testRepository = new TestRepository(ctx);
                    var userRepository = new UserRepository(ctx);
                    var testsToSave = ctx.Tests.Where(t => t.Started != null && t.Finished == null && t.Started < DateTime.Now.AddMinutes(-1 * t.Type.ToQuestionType().Duration().Minutes));
                    foreach (var test in testsToSave)
                    {
                        var user = userRepository.GetUser(test.UserId);
                        var mark = testRepository.MarkTest(test);
                        if (mark >= 80)
                            userRepository.AddCertificate(user, test.Type.ToQuestionType(), test);
                        else
                            userRepository.SetCooldown(user, test.Type.ToQuestionType());
                    }
                }
            }
        }
    }
}
