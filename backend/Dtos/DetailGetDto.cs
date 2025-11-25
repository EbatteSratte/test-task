namespace backend.Dtos;

public class DetailGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int MasterId { get; set; }
}