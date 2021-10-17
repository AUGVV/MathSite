using MathSite.Data;
using MathSite.Functions;
using MathSite.Hubs;
using MathSite.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;

namespace MathSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AzureSecretKey AzureSecretKey = new AzureSecretKey();

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSignalR(hubOptions =>
            {
                hubOptions.MaximumReceiveMessageSize = 5000000;  
            });
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.CurrentDirectory);
            services.AddDbContext<TasksContext>(options => options.UseSqlServer(Configuration.GetConnectionString("TasksConnection")));
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews().AddViewLocalization();
            services.AddAuthentication().AddFacebook(FacebookOptions =>
            {
                FacebookOptions.AppId = AzureSecretKey.TakeSecretKey("FacebookAppID", "9e5eb1a22f324d83914e40c8a6907b60");
                FacebookOptions.AppSecret = AzureSecretKey.TakeSecretKey("FacebookAppSecret", "76d5d06259054f339fba22c5876724f6");
            });
            services.AddAuthentication().AddGoogle(GoogleOptions =>
            {
                GoogleOptions.ClientId = AzureSecretKey.TakeSecretKey("GoogleClientId", "e488c683b464452e989259e0c16ab36f");
                GoogleOptions.ClientSecret = AzureSecretKey.TakeSecretKey("GoogleClientSecret", "bf57a5a63d1c40d28a386841e5ee7b90");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("ru"),
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
 
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapHub<ControlHub>("/hubs");
                endpoints.MapControllerRoute(
                    
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }
    }
}
