using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class MasterCreateDto
    {
        [Required(ErrorMessage = "Номер документа обязателен")]
        [StringLength(200)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public string? Note { get; set; }
        public List<DetailCreateDto>? Details { get; set; }
    }
}
