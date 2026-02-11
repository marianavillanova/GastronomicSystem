using RestaurantAPI.Models;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Repositories;
using RestaurantAPI.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true; // Optional for pretty printing
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inject GastronomicSystemContext
builder.Services.AddDbContext<GastronomicSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RestaurantDB")));

builder.Services.AddScoped<ShiftRepository>();
builder.Services.AddScoped<ShiftService>();
builder.Services.AddAutoMapper(typeof(Program));


// Enable CORS for Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3002")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowFrontend"); // CORS kicks in here 🔓

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Welcome to RestaurantAPI!");

app.Run();
