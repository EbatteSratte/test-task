namespace backend.Dtos
{
    public class MasterGetDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
