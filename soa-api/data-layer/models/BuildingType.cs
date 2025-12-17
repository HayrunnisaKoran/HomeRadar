using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeRadar.Models
{
    /// <summary>
    /// Bina Tipleri tablosu - Daire, Villa, Müstakil vb.
    /// </summary>
    [Table("BuildingTypes")]
    public class BuildingType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Name")]
        public string Name { get; set; } // "Daire", "Villa", "Müstakil", "Dubleks"

        [MaxLength(200)]
        [Column("Description")]
        public string Description { get; set; }

        // Navigation Properties
        public virtual ICollection<Listing> Listings { get; set; }
    }
}

