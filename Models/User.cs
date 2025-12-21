using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeRadar.Models
{
    /// <summary>
    /// Kullanıcılar tablosu - Admin ve User rolleri için
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("LastName")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("Role")]
        public string Role { get; set; } = string.Empty; // "Admin" veya "User"

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Prediction>? Predictions { get; set; }
    }
}

