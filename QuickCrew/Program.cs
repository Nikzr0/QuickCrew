using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;


using QuickCrew.Data;
using QuickCrew.Data.Entities;
using QuickCrew.Extensions;
using QuickCrew.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddTransient<IEmailSender, NullEmailSender>();

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddDbContext<QuickCrewContext>(options =>
    options.UseSqlServer("Server=.;Database=QuickCrew;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    //.AddCookie(IdentityConstants.ApplicationScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

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
    .AddRoles<IdentityRole>() // adds roles
    .AddEntityFrameworkStores<QuickCrewContext>() // adds to db
    .AddApiEndpoints();// adds to swagger


builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();

    app.InitializeDatabase();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapGroup("api/auth")
    .WithTags("Auth")
    .MapCustomIdentityApi();

app.MapControllers();

app.Run();
