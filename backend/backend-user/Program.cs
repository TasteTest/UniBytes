using backend_user.Config;

var builder = WebApplication.CreateBuilder(args);

// Allow environment variables to override connection strings
// Format: ConnectionStrings__DefaultConnection
builder.Configuration.AddEnvironmentVariables();

// Add configurations using extension methods from Config folder
builder.Services.AddControllers();
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRepositoryConfiguration();
builder.Services.AddServiceConfiguration();
builder.Services.AddAutoMapperConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthCheckConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseHttpsRedirection();
app.UseCorsConfiguration();
app.UseAuthorization();
app.MapControllers();
app.UseHealthCheckConfiguration();

app.Run();
