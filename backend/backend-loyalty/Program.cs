using backend_loyalty.Config;

var builder = WebApplication.CreateBuilder(args);

// Allow environment variables to override connection strings
// Format: ConnectionStrings__DefaultConnection
builder.Configuration.AddEnvironmentVariables();

// Add configurations using extension methods from Config folder
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure services using extension methods
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRepositoryConfiguration();
builder.Services.AddServiceConfiguration();
builder.Services.AddAutoMapperConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthCheckConfiguration(builder.Configuration);  // Added builder.Configuration parameter

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();