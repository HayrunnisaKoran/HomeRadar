using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Configuration;

namespace HomeRadar.Data
{
    /// <summary>
    /// Design-time DbContext Factory - Migration komutları için gerekli
    /// </summary>
    public class EmlakContextFactory : IDesignTimeDbContextFactory<EmlakContext>
    {
        public EmlakContext CreateDbContext(string[] args)
        {
            // App.config'den connection string'i oku
            var connectionString = ConfigurationManager.ConnectionStrings["HomeRadarConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Eğer App.config'den okunamazsa, varsayılan connection string kullan
                // NOT: Migration için postgres kullanıcısı kullanılıyor (superuser yetkisi gerekli)
                connectionString = "Host=localhost;Port=5432;Database=HomeRadar_db;Username=postgres;Password=250400";
            }
            
            // Migration için postgres kullanıcısı kullan (kullanıcı henüz oluşturulmamış olabilir)
            // Production'da homeradar_app_user kullanılacak
            if (connectionString.Contains("homeradar_app_user"))
            {
                connectionString = connectionString.Replace("Username=homeradar_app_user", "Username=postgres");
            }

            var optionsBuilder = new DbContextOptionsBuilder<EmlakContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new EmlakContext(optionsBuilder.Options);
        }
    }
}

