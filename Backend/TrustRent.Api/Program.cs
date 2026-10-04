using Microsoft.OpenApi;
using Scalar.AspNetCore;
using TrustRent.Application;
using TrustRent.Infrastructure;
using TrustRent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using TrustRent.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Controllers
// --------------------------------------------------

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()));

// --------------------------------------------------
// Application
// --------------------------------------------------

builder.Services.AddApplication();

// --------------------------------------------------
// Infrastructure
// --------------------------------------------------

builder.Services.AddInfrastructure(
    builder.Configuration);

// --------------------------------------------------
// OpenAPI
// --------------------------------------------------

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste your JWT access token here"
        });

        return Task.CompletedTask;
    });
});


// --------------------------------------------------
// CORS
// --------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// --------------------------------------------------
// Database + Identity seed
// --------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<
            RoleManager<ApplicationRole>>();

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedAsync(
        roleManager,
        userManager,
        app.Configuration);
}

// --------------------------------------------------
// HTTP pipeline
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TrustRent API")
            .WithTheme(ScalarTheme.DeepSpace);
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AngularClient");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();