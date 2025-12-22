using Microsoft.EntityFrameworkCore;
using HomeRadar.Data;
using HomeRadar.Services;
using HomeRadar.Repositories;
using System;

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

// Entity Framework ve PostgreSQL bağlantısı
builder.Services.AddDbContext<EmlakContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HomeRadarConnection")));

// Repositories - Scoped lifetime (her HTTP request için yeni instance)
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();
builder.Services.AddScoped<IBuildingTypeRepository, BuildingTypeRepository>();
builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();

// HTTP Client for ML Service
builder.Services.AddHttpClient<IMLService, MLService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Services - Scoped lifetime
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<AuthService>();

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
