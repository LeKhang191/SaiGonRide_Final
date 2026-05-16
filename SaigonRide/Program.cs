using Microsoft.EntityFrameworkCore;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using SaigonRide.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    if (string.IsNullOrEmpty(connectionString))
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (!string.IsNullOrEmpty(host))
            connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        if (!context.Stations.Any())
        {
            context.Stations.AddRange(
                new Station { Name = "Ben Thanh Station", Location = "District 1", MaxCapacity = 50, CurrentInventory = 40 },
                new Station { Name = "Thao Dien Station", Location = "District 2", MaxCapacity = 50, CurrentInventory = 5 }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed. App will start without DB.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.Run();
}
else
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Run($"http://+:{port}");
}
