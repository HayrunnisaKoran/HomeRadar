using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeRadar.Models
{
    /// <summary>
    /// İlan-Özellik ilişki tablosu (Many-to-Many)
    /// </summary>
    [Table("ListingFeatures")]
    public class ListingFeature
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("ListingId")]
        public int ListingId { get; set; }

        [Required]
        [Column("FeatureId")]
        public int FeatureId { get; set; }

        // Navigation Properties
        [ForeignKey("ListingId")]
        public virtual Listing? Listing { get; set; }

        [ForeignKey("FeatureId")]
        public virtual Feature? Feature { get; set; }
    }
}

