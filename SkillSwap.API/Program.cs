using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkillSwap.API.Hubs;
using SkillSwap.API.Middleware;
using SkillSwap.API.Services;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Core.Interfaces.Services;
using SkillSwap.Infrastructure.Data;
using SkillSwap.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------- Load .env file ----------
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
        var separatorIndex = trimmed.IndexOf('=');
        if (separatorIndex <= 0) continue;
        var envKey = trimmed[..separatorIndex].Trim();
        var envValue = trimmed[(separatorIndex + 1)..].Trim();
        Environment.SetEnvironmentVariable(envKey, envValue);
    }
}

builder.Configuration
    .AddEnvironmentVariables();

// Map env vars to configuration keys
if (Environment.GetEnvironmentVariable("CONNECTION_STRING") is { Length: > 0 } connStr)
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;
if (Environment.GetEnvironmentVariable("JWT_SECRET") is { Length: > 0 } jwtSecret)
    builder.Configuration["Jwt:Secret"] = jwtSecret;
if (Environment.GetEnvironmentVariable("JWT_ISSUER") is { Length: > 0 } jwtIssuer)
    builder.Configuration["Jwt:Issuer"] = jwtIssuer;
if (Environment.GetEnvironmentVariable("JWT_AUDIENCE") is { Length: > 0 } jwtAudience)
    builder.Configuration["Jwt:Audience"] = jwtAudience;
if (Environment.GetEnvironmentVariable("RECOMMENDATIONS_SERVICE_URL") is { Length: > 0 } recServiceUrl)
    builder.Configuration["RecommendationsService:BaseUrl"] = recServiceUrl;

// ---------- EF Core ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// ---------- Identity ----------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ---------- JWT ----------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/chathub"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---------- Repositories ----------
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ISwapRepository, SwapRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITimeTransactionRepository, TimeTransactionRepository>();
builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();

// ---------- Services ----------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<ISwapService, SwapService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITimeTransactionService, TimeTransactionService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IDisputeService, DisputeService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<DbSeeder>();

builder.Services.AddHttpClient("RecommendationsService", (sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["RecommendationsService:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
});

// ---------- SignalR + Controllers ----------
builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

// ---------- Seed Database ----------
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

// ---------- Middleware pipeline ----------
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();
