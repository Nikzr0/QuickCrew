using System;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuickCrew.Data;
using QuickCrew.Data.Entities;

namespace QuickCrew.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("QuickCrewContextConnection") ?? throw new InvalidOperationException("Connection string 'QuickCrewContextConnection' not found.");

            builder.Services.AddDbContext<QuickCrewContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<QuickCrewContext>();

            builder.Services.AddHttpClient("QuickCrewAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7224");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient<ApiService>();
            builder.Services.AddAutoMapper(typeof(Program));

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}