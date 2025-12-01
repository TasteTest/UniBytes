using backend.Config;
using DotNetEnv;

// Load .env.local only when no POSTGRES_* variables are already provided.
// This lets local development use .env.local, while Docker uses its own env.
var hasExternalPostgresConfig = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES_HOST"));
if (!hasExternalPostgresConfig)
{
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
    if (File.Exists(envPath))
    {
        Env.Load(envPath);
    }
}

var builder = WebApplication.CreateBuilder(args);

// Allow environment variables to override connection strings
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Custom configurations
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRepositoryConfiguration();
builder.Services.AddServiceConfiguration(builder.Configuration);
builder.Services.AddAutoMapperConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddHealthCheckConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwaggerConfiguration(app.Environment);

app.UseHttpsRedirection();
app.UseCorsConfiguration();
app.UseAuthorization();

app.MapControllers();
app.UseHealthCheckConfiguration();

app.Run();

