using System;
using System.Linq;
using HomeRadar.Data;
using HomeRadar.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("HomeRadar - Emlak Değerleme Sistemi");
            Console.WriteLine("=====================================\n");

            try
            {
                // Veritabanı bağlantısını test et
                using (var context = new EmlakContext())
                {
                    Console.WriteLine("Veritabanı bağlantısı test ediliyor...");
                    
                    // Veritabanının oluşturulup oluşturulmadığını kontrol et
                    bool canConnect = context.Database.CanConnect();
                    
                    if (canConnect)
                    {
                        Console.WriteLine("✓ Veritabanı bağlantısı başarılı!\n");
                        
                        // Tabloların varlığını kontrol et
                        Console.WriteLine("Tablolar kontrol ediliyor...");
                        var tables = new (string tableName, Func<bool> checkFunc)[]
                        {
                            ("Users", () => { try { var _ = context.Users.Count(); return true; } catch { return false; } }),
                            ("Districts", () => { try { var _ = context.Districts.Count(); return true; } catch { return false; } }),
                            ("BuildingTypes", () => { try { var _ = context.BuildingTypes.Count(); return true; } catch { return false; } }),
                            ("Features", () => { try { var _ = context.Features.Count(); return true; } catch { return false; } }),
                            ("Listings", () => { try { var _ = context.Listings.Count(); return true; } catch { return false; } }),
                            ("ListingFeatures", () => { try { var _ = context.ListingFeatures.Count(); return true; } catch { return false; } }),
                            ("Predictions", () => { try { var _ = context.Predictions.Count(); return true; } catch { return false; } })
                        };

                        foreach (var table in tables)
                        {
                            var exists = table.checkFunc();
                            var status = exists ? "OK" : "YOK";
                            Console.WriteLine($"  - {table.tableName}: {status}");
                        }

                        // Örnek veri sayılarını göster
                        Console.WriteLine("\nVeri İstatistikleri:");
                        Console.WriteLine($"  - Kullanıcılar: {context.Users.Count()}");
                        Console.WriteLine($"  - İlçeler: {context.Districts.Count()}");
                        Console.WriteLine($"  - Bina Tipleri: {context.BuildingTypes.Count()}");
                        Console.WriteLine($"  - Özellikler: {context.Features.Count()}");
                        Console.WriteLine($"  - İlanlar: {context.Listings.Count()}");
                        Console.WriteLine($"  - Tahminler: {context.Predictions.Count()}");
                    }
                    else
                    {
                        Console.WriteLine("✗ Veritabanı bağlantısı başarısız!");
                        Console.WriteLine("\nLütfen şunları kontrol edin:");
                        Console.WriteLine("1. PostgreSQL servisinin çalıştığından emin olun");
                        Console.WriteLine("2. App.config dosyasındaki connection string'i kontrol edin");
                        Console.WriteLine("3. Veritabanının oluşturulduğundan emin olun");
                        Console.WriteLine("\nMigration oluşturmak için:");
                        Console.WriteLine("  Package Manager Console'da:");
                        Console.WriteLine("  Add-Migration InitialCreate");
                        Console.WriteLine("  Update-Database");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Hata oluştu: {ex.Message}");
                Console.WriteLine($"\nDetay: {ex}");
            }

            Console.WriteLine("\nÇıkmak için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}
