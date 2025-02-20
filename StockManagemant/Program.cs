﻿using Microsoft.EntityFrameworkCore;
using StockManagemant.Business.Managers;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories;
using StockManagemant.Business.MappingProfiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext with DI container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and managers
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductManager>();

builder.Services.AddScoped<ReceiptRepository>();
builder.Services.AddScoped<ReceiptManager>();

builder.Services.AddScoped<ReceiptDetailRepository>();
builder.Services.AddScoped<ReceiptDetailManager>();

builder.Services.AddScoped<CategoryRepository>();  
builder.Services.AddScoped<CategoryManager>();

//Register interface with managerr..
builder.Services.AddScoped<ICategoryManager, CategoryManager>();
builder.Services.AddScoped<IProductManager, ProductManager>();
builder.Services.AddScoped<IReceiptDetailManager,ReceiptDetailManager>();
builder.Services.AddScoped<IReceiptManager, ReceiptManager>();

builder.Services.AddAutoMapper(typeof(GeneralMappingProfile));

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout to 30 minutes
    options.Cookie.HttpOnly = true; // Secure the session cookie
    options.Cookie.IsEssential = true; // Make session cookie essential for the application
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware to the pipeline
app.UseSession(); // This is where session handling is enabled

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
