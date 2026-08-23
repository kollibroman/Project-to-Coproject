using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectCowork.Api.Extensions;
using ProjectCowork.Api.Utils;
using ProjectCowork.Core;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure;
using ProjectCowork.Infrastructure.Authorization.Models;
using ProjectCowork.Persistence;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); 
});

builder.Services.AddHttpContextAccessor();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Register all possible permission policies
    foreach (var resource in Enum.GetValues<ResourceType>())
    {
        foreach (var action in Enum.GetValues<ActionType>())
        {
            var policyName = $"Permission.{resource}.{action}";
            options.AddPolicy(policyName, policy => 
                policy.Requirements.Add(new PermissionRequirement(resource, action)));
        }
    }
});
        
builder.Services.AddSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCoreServices(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"))
        .ExcludeFromDescription();
}

app.UseAuthentication();
app.UseAuthorization();

app.AddControllers();

app.UseHttpsRedirection();
app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProjectCoworkDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();