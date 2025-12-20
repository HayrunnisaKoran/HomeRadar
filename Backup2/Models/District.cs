using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeRadar.Models
{
    /// <summary>
    /// İlçeler tablosu - Normalizasyon için
    /// </summary>
    [Table("Districts")]
    public class District
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Name")]
        public string Name { get; set; }

        [MaxLength(50)]
        [Column("City")]
        public string City { get; set; } = "Manisa";

        // Navigation Properties
        public virtual ICollection<Listing> Listings { get; set; }
    }
}

