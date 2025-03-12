using FizzBuzzGame.Infrastructure;
using Microsoft.AspNetCore.Session;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging to the console and a file
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()  // Log level
    .WriteTo.Console()     // Log to console
    .WriteTo.File("logs/fizzbuzz-api.log", rollingInterval: RollingInterval.Day) // Log to file with daily rollovers
    .CreateLogger();

// Add services to the container.
builder.Services.AddControllers();

// Add Infrastructure services (including MongoDB)
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp",
        builder => builder
            .WithOrigins("http://localhost:3000",
                "https://localhost:3000","http://frontend:3000",
                "https://frontend:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

// Add session for temporary storage (used in the controller)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowNextApp");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

// Start logging with Serilog
Log.Information("Application Starting");

// Run the application
try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();  // Ensure that logs are flushed before the application exits
}
