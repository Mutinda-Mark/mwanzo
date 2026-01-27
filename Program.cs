using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using mwanzo.Data;
using mwanzo.Models;
using mwanzo.Services;
using AutoMapper; // AutoMapper namespace
using mwanzo.MappingProfiles; // Your mapping profile namespace
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Database Configuration
// --------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------
// Identity Configuration + Password Policy
// --------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    options.SignIn.RequireConfirmedEmail = true; // require email confirmation
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --------------------
// JWT Authentication
// --------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// --------------------
// AutoMapper Configuration
// --------------------
builder.Services.AddAutoMapper(typeof(AppMappingProfile)); 
// This automatically registers your DTO mappings from AppMappingProfile.cs

// --------------------
// Other Services
// --------------------
builder.Services.AddScoped<AuditService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------
// Build the App
// --------------------
var app = builder.Build();

// --------------------
// Role Seeding (Admin, Teacher, Student, Parent)
// --------------------
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Teacher", "Student", "Parent" };

    try
    {
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding roles: {ex.Message}");
    }
}

// --------------------
// Middleware Pipeline
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // must come before authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
