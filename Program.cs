using Microsoft.EntityFrameworkCore;
using HomeRadar.Data;
using HomeRadar.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session yapılandırması (Rol yönetimi için)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContextAccessor (AuthService için)
builder.Services.AddHttpContextAccessor();

// Custom Services
builder.Services.AddScoped<AuthService>();

// Entity Framework ve PostgreSQL bağlantısı
builder.Services.AddDbContext<EmlakContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HomeRadarConnection")));

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

// Session middleware (Authorization'dan önce olmalı)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
