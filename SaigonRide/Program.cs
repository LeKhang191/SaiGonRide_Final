using Microsoft.EntityFrameworkCore;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using SaigonRide.Services;


var builder = WebApplication.CreateBuilder(args);

// Database service sign up
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVS service
builder.Services.AddControllersWithViews();

// Payment services
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

// config HTTP request pipeline
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

// route config
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
        context.Database.Migrate();

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

app.Run();