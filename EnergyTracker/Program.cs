using EnergyTracker.DataAccess.DataContexts;
using EnergyTracker.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using EnergyTracker.DataAccess.Interfaces;
using EnergyTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for console and Seq
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341")
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// Add EF Core with SQLite
builder.Services.AddDbContext<EnergyDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=energy.db"));

// Register repositories and services
builder.Services.AddScoped<IEnergyReadingRepository, EnergyReadingRepository>();
builder.Services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
builder.Services.AddScoped<EnergyService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls("http://*:80");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
