using CTC.Data;
using CTC.Extensions;
using CTC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



builder.Services.AddControllersWithViews();
builder.Services.AddServiceExtensions(builder.Configuration);
builder.Services.AddIdentityServiceExtensions(builder.Configuration);
builder.Services.AddDistributedMemoryCache();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}");


var scope = app.Services.CreateScope();
var service = scope.ServiceProvider;
try
{
    var context = service.GetRequiredService<CtcDbContext>();
    await context.Database.MigrateAsync();
    var userManager = service.GetRequiredService<UserManager<User>>();
    var roleManager = service.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await SeedData.InitializeUser(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = service.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}
app.Run();
