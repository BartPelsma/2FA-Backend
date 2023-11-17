using _2faBackend.Data;
using _2faBackend.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json"); // Voeg dit toe om de appsettings.json te laden

// Voeg services toe aan de container.
builder.Services.AddControllers();

// Voeg Identity services toe
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Configureer hier je identity-opties indien nodig
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Voeg DbContext toe met connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Voeg OAuth authenticatie toe
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
            ValidIssuer = "https://localhost:7156/", // Jouw Issuer URL
            ValidAudience = "http://localhost:3000/", // Jouw Audience URL (Frontend)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("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"))
        };
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = "67464530436-6ovqv8dkkk06ld07d59uosq58k6h0nmn.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });

// Voeg CORS toe
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Voeg Swagger-configuratie toe
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
});

var app = builder.Build();

// Configureer de HTTP-requestpipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend"); // CORS-middleware toevoegen
app.UseHttpsRedirection();
app.UseAuthentication(); // Authenticatiemiddleware toevoegen
app.UseAuthorization();

app.MapControllers();

app.Run();
