namespace backend.Dtos
{
    public class MasterGetByIdDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }

        public List<DetailDto> Details { get; set; } = new();
    }

    public class DetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
