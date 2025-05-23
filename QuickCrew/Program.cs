using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using QuickCrew.Data;
using QuickCrew.Data.Entities;
using QuickCrew.Extensions;
using QuickCrew.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure both API controllers and MVC views support
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder => builder.WithOrigins("https://localhost:7129") // MVC port
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Database configuration
//builder.Services.AddDbContext<QuickCrewContext>(options =>
//    options.UseSqlServer("Server=.;Database=QuickCrew;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"));

builder.Services.AddDbContext<QuickCrewContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionAPI")));

// Authentication/Authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

// Identity configuration
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<QuickCrewContext>()
    .AddApiEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.InitializeDatabase();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Required for MVC

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowWebApp");

// API Endpoints
app.MapGroup("api/auth")
   .WithTags("Auth")
   .MapCustomIdentityApi();

// MVC Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API Controllers
app.MapControllers();

app.Run();