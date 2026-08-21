namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>Lightweight DTO for product list/collection responses.</summary>
public class ProductListDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public int ItemCount { get; set; }
}
