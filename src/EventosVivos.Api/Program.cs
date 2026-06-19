using EventosVivos.Api.Endpoints;
using EventosVivos.Api.Middleware;
using EventosVivos.Application.Events;
using EventosVivos.Application.Reports;
using EventosVivos.Application.Reservations;
using EventosVivos.Infrastructure;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.SeedData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Infrastructure (EF Core, repositories, time provider)
builder.Services.AddInfrastructure(builder.Configuration);

// Application use cases
builder.Services.AddScoped<CreateEventUseCase>();
builder.Services.AddScoped<ListEventsUseCase>();
builder.Services.AddScoped<CreateReservationUseCase>();
builder.Services.AddScoped<ConfirmPaymentUseCase>();
builder.Services.AddScoped<CancelReservationAdminUseCase>();
builder.Services.AddScoped<CancelReservationBuyerUseCase>();
builder.Services.AddScoped<ListReservationsUseCase>();
builder.Services.AddScoped<GetOccupancyReportUseCase>();

// API-Key authentication for admin endpoints
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>("ApiKey", null);

builder.Services.AddAuthorization(opts =>
    opts.AddPolicy("AdminPolicy", p => p.RequireAuthenticatedUser()));

// CORS: allow Angular dev origin and any configured origin
var frontendOrigin = builder.Configuration["AllowedOrigin"] ?? "http://localhost:4200";
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.WithOrigins(frontendOrigin, "http://localhost:4200", "http://localhost:4300")
         .AllowAnyHeader()
         .AllowAnyMethod()));

builder.Services.AddOpenApi();

var app = builder.Build();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventosVivosDbContext>();
    await db.Database.MigrateAsync();
    await VenueSeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var api = app.MapGroup("/api");
api.MapGroup("/venues").MapVenueEndpoints();
api.MapGroup("/events").MapEventEndpoints();
api.MapGroup("/reservations").MapReservationEndpoints();

app.Run();

public partial class Program { }
