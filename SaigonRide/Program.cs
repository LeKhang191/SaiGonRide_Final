using Microsoft.EntityFrameworkCore;
using SaigonRide.Data;
using SaigonRide.Models.Entities;
using SaigonRide.Services;


var builder = WebApplication.CreateBuilder(args);

// Database service sign up
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVS service
builder.Services.AddControllersWithViews();

// Payment services
builder.Services.AddScoped<IVnPayService, VnPayService>();

var app = builder.Build();

// config HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// route config
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    if (!context.Stations.Any())
    {
        context.Stations.AddRange(
            new Station { Name = "Ben Thanh Station", Location = "District 1", MaxCapacity = 50, CurrentInventory = 40 },
            new Station { Name = "Thao Dien Station", Location = "District 2", MaxCapacity = 50, CurrentInventory = 5 } // Sẽ hiện cảnh báo < 20%
        );
        context.SaveChanges();
    }
}

app.Run();