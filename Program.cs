// TODO add RabbitMQ for async processing of invoice creation and updates
// TODO add Redis for caching of invoice data and improving performance of read operations  
// TODO add Rate limiting especially to auth endpoints
// TODO add pagination on invoice list
// TODO renew and revoke JWT 


using System.Text;
using InvoiceTrackerAPI2.Data;
using InvoiceTrackerAPI2.Mappings;
using InvoiceTrackerAPI2.Repositories;
using InvoiceTrackerAPI2.Repositories.Interfaces;
using InvoiceTrackerAPI2.Services;
using InvoiceTrackerAPI2.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ── Auth ──────────────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// ── Services ──────────────────────────────────────────────────────────────────
// AddScoped - one instance per HTTP request
// AddSingleton with EF core would give threading issues
// AddTransient 
// AppDbContext must be scoped
// TODO AddSingleton vs AddScoped

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<InvoiceMappingProfile>());

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");


builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Migrate on startup ────────────────────────────────────────────────────────
// Database.Migrate() checks for unapplied migrations and runs them
// TODO run as seperate deployment step to avoid race conditions when multiple instances start
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Health / liveness / readiness endpoints ───────────────────────────────────
// GET /healthz          → liveness  (is the process alive?)
// GET /healthz/ready    → readiness (is the DB reachable?)
// GET /healthz/live     → liveness  (alias, for k8s probes)
app.MapHealthChecks("/healthz/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") || check.Name == "database",
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/healthz/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // liveness = process is up, no checks needed
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = WriteHealthResponse
});

app.Run();

static Task WriteHealthResponse(HttpContext ctx, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    var result = System.Text.Json.JsonSerializer.Serialize(new
    {
        status  = report.Status.ToString(),
        checks  = report.Entries.Select(e => new
        {
            name     = e.Key,
            status   = e.Value.Status.ToString(),
            duration = e.Value.Duration.TotalMilliseconds + "ms",
            error    = e.Value.Exception?.Message
        })
    });
    return ctx.Response.WriteAsync(result);
}
