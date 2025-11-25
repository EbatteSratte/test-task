using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("Master")]
    public class Master
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        [Column("Number")]
        public string Number { get; set; } = string.Empty;
        
        [Required]
        [Column("Date")]
        public DateTime Date { get; set; }
        
        [Column("Amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [StringLength(500)]
        [Column("Note")]
        public string? Note { get; set; }
        
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Detail> Details { get; set; } = new List<Detail>();
    }
}
