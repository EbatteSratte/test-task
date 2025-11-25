using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("Detail")]
    public class Detail
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("Amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [Column("MasterId")]
        public int MasterId { get; set; }
        
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("MasterId")]
        public virtual Master Master { get; set; } = null!;
    }
}
