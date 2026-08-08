using Serilog;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stock_Warehouse_Tracking_Project_API.API.Middleware;
using Stock_Warehouse_Tracking_Project_API.Application.Mappings;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Logging;
using Stock_Warehouse_Tracking_Project_API.Application.Services;
using Stock_Warehouse_Tracking_Project_API.Application.Validators;
using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Integrations.Notifications;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Background;
using Stock_Warehouse_Tracking_Project_API.API.Hubs;
using SapNwRfc.Pooling;


var builder = WebApplication.CreateBuilder(args);

// ── Serilog (en başta yapılandırılmalı) ──────────────────────────────────────
builder.AddSerilogConfiguration();

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── SAP Client (Mock | Http | Rfc) ────────────────────────────────────────────
var sapProvider = SapClientConfiguration.GetProvider(builder.Configuration);

switch (sapProvider)
{
    case SapClientProvider.Mock:
        builder.Services.AddScoped<ISapClient, MockSapClient>();
        break;

    case SapClientProvider.Http:
        builder.Services.AddSapHttpClient(builder.Configuration);
        break;

    case SapClientProvider.Rfc:
        builder.Services.Configure<SapRfcOptions>(builder.Configuration.GetSection(SapRfcOptions.SectionName));
        builder.Services.AddSingleton<ISapConnectionPool>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var rfc = cfg.GetSection(SapRfcOptions.SectionName).Get<SapRfcOptions>() ?? new SapRfcOptions();
            var cs = rfc.BuildConnectionString();
            return new SapConnectionPool(
                connectionString: cs,
                poolSize: rfc.PoolSize,
                connectionIdleTimeout: TimeSpan.FromSeconds(rfc.IdleTimeoutSeconds));
        });
        builder.Services.AddScoped<ISapPooledConnection, SapPooledConnection>();
        builder.Services.AddScoped<ISapClient, RfcSapClient>();
        break;

    default:
        throw new InvalidOperationException($"Unsupported SapClient provider: {sapProvider}");
}

// ── Authentication (JWT Bearer) ───────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── CORS (SPA geliştirme / ayrı origin dağıtım) ───────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── HTTP Context erişimi (CurrentUserService için) ────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<INotificationProvider, SendGridNotificationProvider>();
builder.Services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
builder.Services.AddScoped<IStockNotificationService, StockNotificationService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IStockThresholdService, StockThresholdService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IHealthStatusService, HealthStatusService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IOperationLogService, OperationLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<INewStockService, NewStockService>();
builder.Services.AddScoped<IMovementService, MovementService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddHostedService<WeeklyReportBackgroundService>();

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// ── MVC + Swagger ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token giriniz. Örnek: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddCheck<SapHealthCheck>("sap");

var app = builder.Build();

// ── Seed SuperAdmin ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedSuperAdminAsync(db);
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseHealthChecks("/health");
app.UseHealthChecks("/health/sap", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Name == "sap"
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<StockHub>("/hubs/stock");

app.Run();
