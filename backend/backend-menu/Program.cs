using backend_menu.Data;
using backend_menu.Repositories;
using backend_menu.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
DotNetEnv.Env.Load(".env.local");

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?.Replace("${POSTGRES_HOST}", Environment.GetEnvironmentVariable("POSTGRES_HOST"))
    .Replace("${POSTGRES_PORT}", Environment.GetEnvironmentVariable("POSTGRES_PORT"))
    .Replace("${POSTGRES_DB}", Environment.GetEnvironmentVariable("POSTGRES_DB"))
    .Replace("${POSTGRES_USER}", Environment.GetEnvironmentVariable("POSTGRES_USER"))
    .Replace("${POSTGRES_PASSWORD}", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"))
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<MenuDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Services
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();