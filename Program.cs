using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CRN_Technical_Assessment.Application.Interfaces;
using CRN_Technical_Assessment.Application.Services;
using CRN_Technical_Assessment.Application.Validators;
using CRN_Technical_Assessment.Extensions;
using CRN_Technical_Assessment.Infrastructure.Data;
using CRN_Technical_Assessment.Infrastructure.Data.Repositories;
using CRN_Technical_Assessment.Infrastructure.Identity;
using CRN_Technical_Assessment.Infrastructure.Services;
using CRN_Technical_Assessment.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CRN_Technical_Assessment;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Database ──────────────────────────────────────────────────────────
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("ConnectionString"),
                sql => sql.EnableRetryOnFailure()));

        // ── Repository + Unit of Work ─────────────────────────────────────────
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IItemRepository, ItemRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Application Services ──────────────────────────────────────────────
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // ── Infrastructure Services ───────────────────────────────────────────
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // ── FluentValidation ──────────────────────────────────────────────────
        builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

        // ── JWT Authentication ────────────────────────────────────────────────
        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        // ── API Versioning ────────────────────────────────────────────────────
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // ── Controllers ───────────────────────────────────────────────────────
        builder.Services.AddControllers();

        // ── Swagger / OpenAPI ─────────────────────────────────────────────────
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddSwaggerGen();

        // ── CORS ──────────────────────────────────────────────────────────────
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // ── Health Checks ─────────────────────────────────────────────────────
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        // ────────────────────────────────────────────────────────────────────
        var app = builder.Build();
        // ────────────────────────────────────────────────────────────────────

        // ── Global Exception Middleware ───────────────────────────────────────
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // ── Swagger (always enabled for assessment purposes) ───────────────────
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"CRN API {description.GroupName.ToUpper()}");
            }
            options.RoutePrefix = "swagger";
        });

        // ── Security Headers ──────────────────────────────────────────────────
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            await next();
        });

        app.UseHttpsRedirection();
        app.UseCors("DefaultCors");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // ── Health Check Endpoint ─────────────────────────────────────────────
        app.MapHealthChecks("/health");

        // ── Database Seed (Development) ───────────────────────────────────────
        if (app.Environment.IsDevelopment())
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            await DatabaseSeeder.SeedAsync(app.Services, logger);
        }

        app.Logger.LogInformation("CRN Product Management API starting on {Environment}...",
            app.Environment.EnvironmentName);

        await app.RunAsync();
    }
}
