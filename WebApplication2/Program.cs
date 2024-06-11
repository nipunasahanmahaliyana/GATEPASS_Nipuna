using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GatePass.DataAccess.Login;
using GatePass_Project.DataAccess.MyRequest;
using GatePass.DataAccess.ExeApprove;
using GatePass.DataAccess.DoVerify;
using GatePass.DataAccess.Dispatch;
using GatePass.DataAccess.MyReceipt;
using GatePass.DataAccess.ItemTracker;
using GatePass.DataAccess.SystemLocation;
using GatePass.DataAccess.ItemCategory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add logging to the console
builder.Logging.AddConsole();

// Add a SqlConnection to the DI container
builder.Services.AddSingleton(new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    // Set a timeout for the session, if needed
    options.IdleTimeout = TimeSpan.FromMinutes(30); // adjust as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
    });

// Adding the Repositories
builder.Services.AddScoped<LoginRepository>();
builder.Services.AddScoped<NewRequestRepository>();
builder.Services.AddScoped<MyRequestRepository>();
builder.Services.AddScoped<ExeApproveRepository>();
builder.Services.AddScoped<DoVerifyRepository>();
builder.Services.AddScoped<DispatchRepository>();
builder.Services.AddScoped<MyReceiptRepository>();
builder.Services.AddScoped<ItemTrackerRepository>();
builder.Services.AddScoped<SystemLocationRepository>();
builder.Services.AddScoped<ItemCategoryRepository>();
builder.Services.AddScoped<NonSLTRepository>();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
