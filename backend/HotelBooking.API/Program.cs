using HotelBooking.Infrastructure;
using HotelBooking.Infrastructure.Identity;
using HotelBooking.Application.Interfaces;
using HotelBooking.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS configuration. User requested to "allow absolutely all traffic".
// We provide an override env var ALLOW_ALL_CORS=true for production toggle.
var corsPolicy = "DevCors";
var allowAll = string.Equals(Environment.GetEnvironmentVariable("ALLOW_ALL_CORS"), "true", StringComparison.OrdinalIgnoreCase);

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, p =>
    {
        if (allowAll)
        {
            // Fully permissive (no credentials) – safest broad mode for debugging frontends from any origin
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
        else
        {
            // Minimal curated list (can extend via EXTRA_CORS_ORIGINS if needed)
            var allowedOrigins = new List<string>
            {
                "http://localhost:5500",
                "http://127.0.0.1:5500",
                "http://localhost:3000",
                "https://vlad3958.github.io"
            };
            var extraCors = Environment.GetEnvironmentVariable("EXTRA_CORS_ORIGINS");
            if (!string.IsNullOrWhiteSpace(extraCors))
            {
                foreach (var origin in extraCors.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = origin.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed) && !allowedOrigins.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                        allowedOrigins.Add(trimmed);
                }
            }
            p.WithOrigins(allowedOrigins.ToArray())
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
        }
    });
});

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    // Ignore cycles like Room -> Bookings -> Room to prevent System.Text.Json.JsonException
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "HotelBooking API", Version = "v1" });
    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            securityScheme, new List<string>()
        }
    });
});

// Configure EF Core with MySQL (Pomelo)
// Heroku ClearDB provides CLEARDB_DATABASE_URL (e.g. mysql://user:pass@host/db?reconnect=true)
// Some platforms (Render) provide DATABASE_URL.
string? connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

string? TryBuildMySqlFromUrl(string url)
{
    if (string.IsNullOrWhiteSpace(url)) return null;
    // Accept mysql:// or CLEARDB style
    if (!url.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase)) return null;
    try
    {
        var uri = new Uri(url);
        var db = uri.AbsolutePath.TrimStart('/');
        var userInfo = uri.UserInfo.Split(':');
        var user = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? "");
        var pass = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? "");
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 3306;
        return $"Server={host};Port={port};Database={db};User Id={user};Password={pass};SslMode=Preferred;CharSet=utf8mb4;";
    }
    catch
    {
        return null; // let fallback handle error/logging
    }
}

// If DATABASE_URL is a mysql:// URL convert it
var parsedFromDbUrl = TryBuildMySqlFromUrl(connectionString ?? string.Empty);
if (parsedFromDbUrl != null)
    connectionString = parsedFromDbUrl;

if (string.IsNullOrWhiteSpace(connectionString))
{
    var clearDbUrl = Environment.GetEnvironmentVariable("CLEARDB_DATABASE_URL");
    var parsedClear = TryBuildMySqlFromUrl(clearDbUrl ?? string.Empty);
    if (parsedClear != null)
        connectionString = parsedClear;
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    // Fallback to appsettings for local development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found and no database URL env var set.");
}

builder.Services.AddDbContext<HotelBookingDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Infrastructure repositories
builder.Services.AddScoped<HotelBooking.Infrastructure.Interfaces.IClient, HotelBooking.Infrastructure.Repositories.ClientRepository>();
builder.Services.AddScoped<HotelBooking.Infrastructure.Interfaces.IAdmin, HotelBooking.Infrastructure.Repositories.AdminRepository>();
builder.Services.AddScoped<HotelBooking.Infrastructure.Interfaces.IHotel, HotelBooking.Infrastructure.Repositories.AdminRepository>();
builder.Services.AddScoped<HotelBooking.Infrastructure.Interfaces.IRoom, HotelBooking.Infrastructure.Repositories.AdminRepository>();

// Application services
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IRoomService, RoomService>();
// Identity + JWT Authentication
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<HotelBookingDbContext>()
    .AddSignInManager<SignInManager<ApplicationUser>>()
    .AddDefaultTokenProviders();

// JWT configuration - prefer environment variables for production (Render)
var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? jwtSection.GetValue<string>("Key") 
    ?? throw new InvalidOperationException("JWT secret key missing (set JWT_SECRET_KEY env var or Jwt:Key in appsettings)");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? jwtSection.GetValue<string>("Issuer");
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? jwtSection.GetValue<string>("Audience");

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
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };
        // Custom JSON for 401 (not authenticated) and 403 (forbidden)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Suppress default WWW-Authenticate header-only response
                context.HandleResponse();
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var payload = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "Потрібна авторизація: відсутній або недійсний токен (вы не зарегистрированы / не вошли)",
                        code = 401
                    });
                    return context.Response.WriteAsync(payload);
                }
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = "Доступ заборонено: недостатньо прав (нужна роль Admin)",
                    code = 403
                });
                return context.Response.WriteAsync(payload);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
});

// SignInManager requires IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Heroku provides PORT env var; configure Kestrel binding accordingly if set
var herokuPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(herokuPort))
{
    app.Urls.Add($"http://0.0.0.0:{herokuPort}");
}

app.UseCors(corsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelBooking API v1");
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed basic roles (Admin, User) if they don't exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("Startup");
    try
    {
        // Ensure database (schema + Identity tables) is migrated
        var db = services.GetRequiredService<HotelBookingDbContext>();
        db.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = new[] { "Admin", "User" };
        foreach (var r in roles)
        {
            var exists = roleManager.RoleExistsAsync(r).GetAwaiter().GetResult();
            if (!exists)
            {
                var result = roleManager.CreateAsync(new ApplicationRole { Name = r }).GetAwaiter().GetResult();
                if (!result.Succeeded)
                {
                    logger.LogWarning("Failed to create role {Role}: {Errors}", r, string.Join(';', result.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogInformation("Created role {Role}", r);
                }
            }
        }

        // Seed default admin user
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
            ?? builder.Configuration["Admin:Email"]
            ?? "admin@example.com";
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
            ?? builder.Configuration["Admin:Password"]
            ?? "Admin123!"; // Make sure to change in production
        var existingAdmin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsAdmin = true
            };
            var createAdmin = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
            if (!createAdmin.Succeeded)
            {
                logger.LogWarning("Failed to create default admin user: {Errors}", string.Join(';', createAdmin.Errors.Select(e => e.Description)));
            }
            else
            {
                var addRole = userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                if (!addRole.Succeeded)
                {
                    logger.LogWarning("Failed to assign Admin role to default admin: {Errors}", string.Join(';', addRole.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogInformation("Seeded default admin user {Email}", adminEmail);
                }
            }
        }
        else
        {
            // Ensure existing admin has Admin role
            var inRole = userManager.IsInRoleAsync(existingAdmin, "Admin").GetAwaiter().GetResult();
            if (!inRole)
            {
                var addRole = userManager.AddToRoleAsync(existingAdmin, "Admin").GetAwaiter().GetResult();
                if (!addRole.Succeeded)
                {
                    logger.LogWarning("Failed to ensure Admin role for existing admin {Email}: {Errors}", adminEmail, string.Join(';', addRole.Errors.Select(e => e.Description)));
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error while migrating database / seeding roles");
        // Do not rethrow to allow app to start (optional). If you prefer fail-fast, uncomment:
        // throw;
    }
}

app.Run();
