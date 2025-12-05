

/*
 * 
Regen EntityFramework with SQL server


Scaffold-DbContext "Data Source=112.78.2.42,1433;Initial Catalog=gol82750_webgoli.com.vn;User ID=gol82750_webgoli.com.vn;Password=Webgoli121@;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force

Data Source=112.78.2.42,1433;Initial Catalog=gol82750_webgoli.com.vn;User ID=gol82750_webgoli.com.vn;Password=Webgoli121@;Trust Server Certificate=True
 * 
 */


using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;
using TiramisuShop.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


// Database server connection
//builder.Services.AddDbContext<Gol82750WebgoliComVnContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("GoliFresh")));



// Add Runtime Compilation
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");


// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.LoginPath = "/Admin/User/Login";
        options.AccessDeniedPath = "/";
    });

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(45);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Config Lowercase
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);


/*
 *  BUILD APP
 */
var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseSession();
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=User}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();