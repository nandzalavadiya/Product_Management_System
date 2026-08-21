namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>DTO returned for a single product with full details including items.</summary>
public class ProductResponseDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public List<ItemDto> Items { get; set; } = new();
}
