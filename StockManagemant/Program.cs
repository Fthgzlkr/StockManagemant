﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories;
using StockManagemant.Business.MappingProfiles;
using StockManagemant.Web.Extensions;
using StockManagemant.DataAccess.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Bağlantı dizesi bulunamadı! Lütfen appsettings.json içinde doğru yapılandırmayı yaptığınızdan emin olun.");
}

try
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
catch (Exception ex)
{
    Console.WriteLine($"Veritabanına bağlanırken hata oluştu: {ex.Message}");
    throw;
}

// ✅ Generic Repository Bağımlılığını Ekleme
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ✅ Repository ve Managerları Reflection ile Ekleyelim
builder.Services.AddRepositoriesAndManagers();

// ✅ AutoMapper Bağlantısı
builder.Services.AddAutoMapper(typeof(GeneralMappingProfile));

// ✅ Cookie Authentication Ekle
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// ✅ MVC Controller'ları ve View'leri Yükleme
builder.Services.AddControllersWithViews();

// ✅ Session Services
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// ✅ Authentication ve Authorization sıralı
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();