using System.ComponentModel.DataAnnotations;

namespace backend.Dtos
{
    public class DetailUpdateDto
    {
        [Required(ErrorMessage = "Наименование обязательно")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Сумма обязательна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0")]
        public decimal Amount { get; set; }
    }
}