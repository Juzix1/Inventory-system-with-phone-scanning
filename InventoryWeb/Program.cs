using InventoryLibrary.Data;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Services;
using InventoryLibrary.Services.Interfaces;
using InventoryLibrary.Services.Location;
using InventoryWeb.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBootstrapBlazor();

builder.Services.AddScoped<IInventoryService,InventoryService>();
builder.Services.AddScoped<ITypeService,TypeService>();
builder.Services.AddScoped<IConditionService,ConditionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("InventoryAPI")));


builder.Services.AddControllers();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MyDbContext>();
        // This will create the database if it doesn't exist
        _ = context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

var sharedImagesPath = Path.Combine(
    Directory.GetParent(app.Environment.ContentRootPath)!.FullName,
    "Images"
);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(sharedImagesPath),
    RequestPath = "/images"
});
app.UseRouting();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
    _ = endpoints.MapRazorComponents<App>().AddInteractiveServerRenderMode();
});

app.Run();
