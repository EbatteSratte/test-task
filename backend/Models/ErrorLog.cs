using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Column("ErrorType")]
        public string ErrorType { get; set; } = string.Empty;
        
        [Required]
        [Column("ErrorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;
        
        [Column("StackTrace")]
        public string? StackTrace { get; set; }
        
        [StringLength(100)]
        [Column("UserAction")]
        public string? UserAction { get; set; }
        
        [StringLength(50)]
        [Column("EntityType")]
        public string? EntityType { get; set; }
        
        [StringLength(50)]
        [Column("EntityId")]
        public string? EntityId { get; set; }
        
        [Column("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        [Column("UserId")]
        public string? UserId { get; set; }
        
        [StringLength(45)]
        [Column("IpAddress")]
        public string? IpAddress { get; set; }
    }
}
