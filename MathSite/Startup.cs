using MathSite.Data;
using MathSite.Functions;
using MathSite.Hubs;
using MathSite.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Localization;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                 facebookOptions.AppId = AzureSecretKey.TakeSecretKey("FacebookAppID", "9e5eb1a22f324d83914e40c8a6907b60");
                 facebookOptions.AppSecret = AzureSecretKey.TakeSecretKey("FacebookAppSecret", "76d5d06259054f339fba22c5876724f6");
            });
          //  services.AddAuthentication().AddTwitter(twitterOptions =>
          //  {
          //        twitterOptions.ConsumerKey = "";
          //        twitterOptions.ConsumerSecret = "";
           //       twitterOptions.RetrieveUserDetails = true;
           // });
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
