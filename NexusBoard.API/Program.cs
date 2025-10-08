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

// Configure Database
builder.Services.AddDbContext<NexusBoardDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("NexusBoard.API")));


// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-key-that-needs-to-be-at-least-32-characters-long-for-security";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "NexusBoard",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "NexusBoard",
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

// Configure CORS for Angular app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy
            .WithOrigins("http://localhost:4200", "http://192.168.0.150:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NexusBoardDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();