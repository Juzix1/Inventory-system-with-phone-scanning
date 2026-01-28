using InventoryLibrary.Data;
using InventoryLibrary.Services;
using InventoryLibrary.Services.data;
using InventoryLibrary.Services.Interfaces;
using InventoryLibrary.Services.Location;
using InventoryWeb.Components;
using InventoryWeb.Services;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using InventoryLibrary.Services.Logs;




var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();

// Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "auth_token";
        option.LoginPath = "/login";
        option.LogoutPath = "/logout";
        option.Cookie.MaxAge = TimeSpan.FromMinutes(30);
        option.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();



// Inventory
builder.Services.AddScoped<IInventoryService,InventoryService>();
builder.Services.AddScoped<ITypeService,TypeService>();
builder.Services.AddScoped<IConditionService,ConditionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IStocktakeService, StocktakeService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IHistoricalDataService, HistoricalDataService>();

// Account
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IPasswordService,PasswordService>();
// Settings
builder.Services.AddScoped<ISettingsService, SettingsService>();

// Analytics
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Logging
builder.Services.AddScoped(typeof(IInventoryLogger<>), typeof(InventoryLogger<>));

// Log Reader
builder.Services.AddScoped<ILogReaderService, LogReaderService>();


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

    var baseUri = navigationManager.BaseUri;
    if (baseUri.Contains("0.0.0.0"))
    {
        baseUri = baseUri.Replace("0.0.0.0", "localhost");
    }

    var client = new HttpClient
    {
        BaseAddress = new Uri(baseUri)
    };

    var context = httpContextAccessor.HttpContext;
    var cookie = context?.Request.Headers["Cookie"].ToString();

    if (!string.IsNullOrEmpty(cookie))
    {
        client.DefaultRequestHeaders.Add("Cookie", cookie);
    }

    return client;
});


builder.Services.AddDbContextFactory<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("InventoryAPI")));


builder.Services.AddControllers();

builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error", createScopeForErrors: true);
    _ = app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MyDbContext>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
    _ = endpoints.MapRazorComponents<App>().AddInteractiveServerRenderMode();
});


app.Run();
