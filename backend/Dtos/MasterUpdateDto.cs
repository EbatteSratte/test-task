using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class MasterUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Number { get; set; } = string.Empty;
        
        [Required]
        public DateTime Date { get; set; }
        
        [StringLength(500)]
        public string? Note { get; set; }
    }
}