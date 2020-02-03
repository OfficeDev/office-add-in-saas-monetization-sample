// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApp
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Web;
    using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
    using SaaSSampleWebApp.Authorization;
    using SaaSSampleWebApp.Extensions;
    using SaaSSampleWebApp.Filters;
    using SaaSSampleWebApp.Services;

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
            var azureOptions = new AzureAdOptions();
            Configuration.Bind("AzureAd", azureOptions);

            var saaSWebApiOptions = new Iconfiguration();
            Configuration.Bind("SaaSWebApi", saaSWebApiOptions);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddOptions();

            // Token acquisition service based on MSAL.NET
            // and chosen token cache implementation
            services.AddMicrosoftIdentityPlatformAuthentication(Configuration)
               .AddMsal(Configuration, $"{azureOptions.GraphScopes}".Split(new[] { ' ' }))
               .AddInMemoryTokenCaches();

            // Add Graph
            services.AddApiService(Configuration);

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
                options.Filters.Add(new AuthExceptionFilter());
            });

            services.AddAuthorization(option =>
            {
                option.AddPolicy("ISVAdmin", policy => policy.Requirements.Add(new ISVAdminRequirement("ISVAdmin")));
            });

            services.AddSingleton<IAuthorizationHandler, ISVAdminHandler>();

            services.AddHttpClient();
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
