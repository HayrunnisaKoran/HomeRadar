using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeRadar.Models
{
    /// <summary>
    /// Yapay Zeka Tahminleri tablosu - Kullanıcı sorguları ve ML sonuçları
    /// </summary>
    [Table("Predictions")]
    public class Prediction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign Keys
        [Column("UserId")]
        public int? UserId { get; set; } // Nullable - misafir kullanıcılar da sorgu yapabilir

        [Column("ListingId")]
        public int? ListingId { get; set; } // Nullable - yeni bir ev için tahmin yapılabilir

        // Kullanıcı Girdileri
        [Required(ErrorMessage = "İlçe seçimi zorunludur.")]
        [Display(Name = "İlçe")]
        [Column("DistrictId")]
        public int DistrictId { get; set; }

        [Required(ErrorMessage = "Oda sayısı zorunludur.")]
        [Display(Name = "Oda Sayısı")]
        [Column("RoomCount")]
        public int RoomCount { get; set; }

        [Required(ErrorMessage = "Metrekare bilgisi zorunludur.")]
        [Display(Name = "Metrekare")]
        [Column("SquareMeters", TypeName = "decimal(10,2)")]
        public decimal SquareMeters { get; set; }

        [Required(ErrorMessage = "Bina yaşı zorunludur.")]
        [Display(Name = "Bina Yaşı")]
        [Column("BuildingAge")]
        public int BuildingAge { get; set; }

        [Column("BuildingTypeId")]
        public int? BuildingTypeId { get; set; }

        // ML Model Sonuçları
        [Required]
        [Column("PredictedPriceMin", TypeName = "decimal(18,2)")]
        public decimal PredictedPriceMin { get; set; }

        [Required]
        [Column("PredictedPriceMax", TypeName = "decimal(18,2)")]
        public decimal PredictedPriceMax { get; set; }

        [Required]
        [Column("PredictedPriceAvg", TypeName = "decimal(18,2)")]
        public decimal PredictedPriceAvg { get; set; }

        [MaxLength(50)]
        [Column("ModelName")]
        public string? ModelName { get; set; } // "LinearRegression", "DecisionTree"

        [Column("ConfidenceScore", TypeName = "decimal(5,2)")]
        public decimal? ConfidenceScore { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ListingId")]
        public virtual Listing? Listing { get; set; }

        [ForeignKey("DistrictId")]
        public virtual District? District { get; set; }

        [ForeignKey("BuildingTypeId")]
        public virtual BuildingType? BuildingType { get; set; }
    }
}

