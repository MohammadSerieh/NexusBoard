using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexusBoard.Infrastructure.Data;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NexusBoard API", Version = "v1" });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// --- Database Configuration ---
string? connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(connectionString))
{
    // Convert postgres:// URL to Npgsql connection string
    connectionString = ConvertDatabaseUrlToNpgsql(connectionString);
    Console.WriteLine("Using DATABASE_URL environment variable");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine("Using connection string from appsettings.json");
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string not found.");
}

builder.Services.AddDbContext<NexusBoardDbContext>(options =>
    options.UseNpgsql(connectionString,
        b => b.MigrationsAssembly("NexusBoard.API")));

// --- JWT Authentication ---
var jwtSecret = Environment.GetEnvironmentVariable("Jwt__Secret")
    ?? builder.Configuration["Jwt:Secret"]
    ?? "your-super-secret-key-that-needs-to-be-at-least-32-characters-long-for-security";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? builder.Configuration["Jwt:Issuer"] ?? "NexusBoard",
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? builder.Configuration["Jwt:Audience"] ?? "NexusBoard",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Register Application Services
builder.Services.AddScoped<NexusBoard.API.Interfaces.IServices.IAuthService, NexusBoard.API.Services.AuthService>();
builder.Services.AddScoped<NexusBoard.API.Interfaces.IServices.ITeamService, NexusBoard.API.Services.TeamService>();
builder.Services.AddScoped<NexusBoard.API.Interfaces.IServices.IProjectService, NexusBoard.API.Services.ProjectService>();
builder.Services.AddScoped<NexusBoard.API.Interfaces.IServices.IWorkItemService, NexusBoard.API.Services.WorkItemService>();

// Register Repositories
builder.Services.AddScoped<NexusBoard.API.Interfaces.IRepositories.IAuthRepository, NexusBoard.API.Repositories.AuthRepository>();
builder.Services.AddScoped<NexusBoard.API.Interfaces.IRepositories.ITeamRepository, NexusBoard.API.Repositories.TeamRepository>();
builder.Services.AddScoped<NexusBoard.API.Interfaces.IRepositories.IProjectRepository, NexusBoard.API.Repositories.ProjectRepository>();
builder.Services.AddScoped<NexusBoard.API.Interfaces.IRepositories.IWorkItemRepository, NexusBoard.API.Repositories.WorkItemRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins("http://localhost:4200", "http://192.168.0.150:4200")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
            else
            {
                policy.SetIsOriginAllowed(origin =>
                    origin.Contains("vercel.app") || origin.Contains("localhost"))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        });
});

var app = builder.Build();

Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database on startup
Console.WriteLine("Starting database migration...");
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NexusBoardDbContext>();
    await context.Database.MigrateAsync();
    Console.WriteLine("Database migration completed successfully!");
}

Console.WriteLine("Application starting...");
app.Run();


// --- Helper Method ---
static string ConvertDatabaseUrlToNpgsql(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}