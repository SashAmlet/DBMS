using WebApp.Repository;
using WebApp.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Constants.BasePath = @"C:\Users\ostre\OneDrive\Books\4th_course\IT\LAB1-DBMS\Storage";

builder.Services.AddSingleton<IDatabaseRepository, DatabaseRepository>();

builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<ITableService, TableService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Database}/{action=Index}/{id?}");

app.Run();
