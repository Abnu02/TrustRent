using Scalar.AspNetCore;
using TrustRent.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTER SERVICES
builder.Services.AddControllers();

// Add Infrastructure & Application Services (Clean Architecture)
builder.Services.AddInfrastructureServices();

// Enable CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// OpenAPI / Swagger Documentation
builder.Services.AddOpenApi();

var app = builder.Build();

// 2. CONFIGURE HTTP PIPELINE
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Interactive Scalar UI at /scalar/v1
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Map All Controllers
app.MapControllers();

app.Run();
