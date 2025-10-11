using Hyre.API.Data;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.Auth;
using Hyre.API.Models;
using Hyre.API.Repositories;
using Hyre.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


// JWT Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]))
        };

    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobRepository, JobRepository>();

builder.Services.AddAuthorization();



var app = builder.Build();

async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = {
        "Recruiter", "HR", "Interviewer", "Reviewer",
        "Admin", "SuperAdmin", "Candidate", "Viewer"
    };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

//Log request details
app.Use(async (context, next) =>
{
    Console.WriteLine("\n========== INCOMING REQUEST ==========");
    Console.WriteLine($"Time: {DateTime.Now:HH:mm:ss}");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine($"QueryString: {context.Request.QueryString}");

    Console.WriteLine("Headers:");
    foreach (var header in context.Request.Headers)
    {
        if (header.Key.ToLower() == "authorization")
        {
            var value = header.Value.ToString();
            Console.WriteLine($"  {header.Key}: {(string.IsNullOrEmpty(value) ? "MISSING" : value.Substring(0, Math.Min(50, value.Length)) + "...")}");
        }
        else
        {
            Console.WriteLine($"  {header.Key}: {header.Value}");
        }
    }

    await next();

    Console.WriteLine($"Response Status: {context.Response.StatusCode}");
    Console.WriteLine($"User Authenticated: {context.User?.Identity?.IsAuthenticated}");
    Console.WriteLine("========== REQUEST COMPLETE ==========\n");
});

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRolesAsync(roleManager);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapScalarApiReference();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
