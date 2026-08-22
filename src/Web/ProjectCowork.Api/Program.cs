using Microsoft.EntityFrameworkCore;
using ProjectCowork.Api.Extensions;
using ProjectCowork.Api.Utils;
using ProjectCowork.Core;
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

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
        
builder.Services.AddSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

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