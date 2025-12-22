using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeRadar.Models
{
    /// <summary>
    /// Özellikler tablosu - Otopark, Havuz, Asansör vb.
    /// </summary>
    [Table("Features")]
    public class Feature
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty; // "Balkon", "Asansör", "Garaj", "Otopark", "Havuz", "Güvenlik"

        [MaxLength(200)]
        [Column("Description")]
        public string? Description { get; set; }

        // Navigation Properties - Many-to-Many ilişki için
        public virtual ICollection<ListingFeature>? ListingFeatures { get; set; }
    }
}

