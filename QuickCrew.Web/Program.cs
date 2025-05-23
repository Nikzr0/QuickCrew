using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
//using QuickCrew.Data;

namespace QuickCrew.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient("QuickCrewAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7224");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient<ApiService>();
            builder.Services.AddAutoMapper(typeof(Program));


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}