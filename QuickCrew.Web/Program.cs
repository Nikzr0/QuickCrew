using System;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuickCrew.Data;
using QuickCrew.Data.Entities;
using Microsoft.AspNetCore.Http;
using QuickCrew.Web.Services;

namespace QuickCrew.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("QuickCrewContextConnection") ?? throw new InvalidOperationException("Connection string 'QuickCrewContextConnection' not found.");

            builder.Services.AddDbContext<QuickCrewContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 4;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<QuickCrewContext>();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<QuickCrew.Web.Services.AuthHeaderHandler>();

            builder.Services.AddHttpClient("QuickCrewAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7224");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<QuickCrew.Web.Services.AuthHeaderHandler>();

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

            app.UseSession();

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