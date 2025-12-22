using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeRadar.Models
{
    /// <summary>
    /// İlanlar tablosu - Ana emlak verileri
    /// </summary>
    [Table("Listings")]
    public class Listing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign Keys
        [Required]
        [Column("DistrictId")]
        public int DistrictId { get; set; }

        [Required]
        [Column("BuildingTypeId")]
        public int BuildingTypeId { get; set; }

        // Fiyat ve Temel Bilgiler
        [Required]
        [Column("Price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column("SquareMeters", TypeName = "decimal(10,2)")]
        [Range(1, 10000, ErrorMessage = "Metrekare 1 ile 10000 arasında olmalıdır")]
        public decimal SquareMeters { get; set; }

        [Required]
        [Column("RoomCount")]
        public int RoomCount { get; set; } // Oda sayısı (3+1 için 3)

        [Required]
        [Column("SalonCount")]
        public int SalonCount { get; set; } // Salon sayısı (3+1 için 1)

        [Column("BathroomCount")]
        public int? BathroomCount { get; set; }

        [MaxLength(50)]
        [Column("Floor")]
        public string? Floor { get; set; } // "Zemin", "5. Kat", "Kot 1"

        [Required]
        [Column("BuildingAge")]
        public int BuildingAge { get; set; }

        [Column("Aidat", TypeName = "decimal(10,2)")]
        public decimal? Aidat { get; set; }

        // Detay Bilgileri
        [MaxLength(50)]
        [Column("HeatingType")]
        public string? HeatingType { get; set; } // "Doğalgaz", "Kombi", "Soba"

        [MaxLength(50)]
        [Column("Direction")]
        public string? Direction { get; set; } // "Kuzey", "Güney", "Doğu", "Batı"

        [MaxLength(50)]
        [Column("BuildingStatus")]
        public string? BuildingStatus { get; set; } // "Sıfır", "İkinci El", "Yeni"

        [MaxLength(50)]
        [Column("UsageStatus")]
        public string? UsageStatus { get; set; } // "Boş", "Kiracılı"

        [MaxLength(50)]
        [Column("DeedStatus")]
        public string? DeedStatus { get; set; } // "Kat Mülkiyetli", "Tapu Durumu"

        [MaxLength(50)]
        [Column("FurnitureStatus")]
        public string? FurnitureStatus { get; set; } // "Eşyalı", "Boş"

        // Boolean Özellikler (String olarak saklanabilir, ama bool daha iyi)
        [Column("HasBalcony")]
        public bool? HasBalcony { get; set; }

        [Column("HasElevator")]
        public bool? HasElevator { get; set; }

        [Column("HasGarage")]
        public bool? HasGarage { get; set; }

        [Column("IsInComplex")]
        public bool? IsInComplex { get; set; }

        [Column("HasSecurity")]
        public bool? HasSecurity { get; set; }

        [Column("IsCreditSuitable")]
        public bool? IsCreditSuitable { get; set; }

        [Column("IsExchangeable")]
        public bool? IsExchangeable { get; set; }

        [MaxLength(200)]
        [Column("Neighborhood")]
        public string? Neighborhood { get; set; }

        [Column("ListingDate")]
        public DateTime ListingDate { get; set; } = DateTime.UtcNow;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("DistrictId")]
        public virtual District? District { get; set; }

        [ForeignKey("BuildingTypeId")]
        public virtual BuildingType? BuildingType { get; set; }

        public virtual ICollection<ListingFeature>? ListingFeatures { get; set; }
        public virtual ICollection<Prediction>? Predictions { get; set; }
    }
}

