
using InventoryLibrary.Data;
using InventoryLibrary.Services;
using InventoryLibrary.Services.Interfaces;
using InventoryLibrary.Services.Logs;
using InventoryWeb.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // HTTP
     options.ListenAnyIP(7164, listenOptions => listenOptions.UseHttps()); // HTTPS jeśli potrzebne
});

// Add services to the container.
var MyOrigins = "_myAllowOrigins";

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IHistoricalDataService, HistoricalDataService>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<ITypeService, TypeService>();
builder.Services.AddScoped<IConditionService, ConditionService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<ILocationService, InventoryLibrary.Services.Location.LocationService>();

// Logger
builder.Services.AddScoped(typeof(IInventoryLogger<>), typeof(InventoryLogger<>));



builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("InventoryAPI")));


builder.Services.AddCors(options => {
    options.AddPolicy(name: MyOrigins,
        policy => {
            policy.WithOrigins("http://localhost:5000", "http://localhost:7164", "http://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MyDbContext>();
        // This will create the database if it doesn't exist
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();

}

app.UseHttpsRedirection();

app.UseCors(MyOrigins);
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();


app.Run();
