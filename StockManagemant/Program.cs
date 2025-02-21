using Microsoft.EntityFrameworkCore;
using StockManagemant.Business.Managers;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories;
using StockManagemant.Business.MappingProfiles;

var builder = WebApplication.CreateBuilder(args);


//Önce appsetting i ardından secret ı yükle 
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();


// Bağlantıyı alırken, User Secrets'in de yüklenmiş olmasını sağlıyoruz
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Bağlantı dizesi bulunamadı! Lütfen User Secrets veya appsettings.json içinde doğru yapılandırmayı yaptığınızdan emin olun.");
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



// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext with DI container

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
