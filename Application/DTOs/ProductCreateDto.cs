namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>DTO for creating a new product.</summary>
public class ProductCreateDto
{
    public string ProductName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public List<ItemDto> Items { get; set; } = new();
}
