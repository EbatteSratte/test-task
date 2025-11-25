using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class DetailBulkCreateDto
    {
        [Required(ErrorMessage = "Список деталей обязателен")]
        [MinLength(1, ErrorMessage = "Должна быть хотя бы одна деталь")]
        public List<DetailCreateDto> Details { get; set; } = new();
    }
}
