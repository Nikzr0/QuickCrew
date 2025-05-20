using System.Net.Http.Headers;

namespace QuickCrew.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавяне на HttpClient - "QuickCrewAPI"
            builder.Services.AddHttpClient("QuickCrewAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7224");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient<ApiService>();

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