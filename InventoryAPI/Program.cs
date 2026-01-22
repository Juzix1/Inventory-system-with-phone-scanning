using InventoryLibrary.Data;
using InventoryLibrary.Services;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var MyOrigins = "_myAllowOrigins";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IHistoricalDataService, HistoricalDataService>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<ITypeService, TypeService>();
builder.Services.AddScoped<IConditionService, ConditionService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();



builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("InventoryAPI")));


builder.Services.AddCors(options => {
    options.AddPolicy(name: MyOrigins,
        policy => {
            policy.WithOrigins("http://localhost:5000", "http://localhost:8080","http://localhost:3000");
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(MyOrigins);

app.Run();
