namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>DTO for updating an existing product.</summary>
public class ProductUpdateDto
{
    public string ProductName { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
    public List<ItemDto> Items { get; set; } = new();
}
