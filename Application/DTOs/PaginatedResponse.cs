namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>Paginated response wrapper including pagination metadata.</summary>
public class PaginatedResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public PaginationMeta Pagination { get; set; } = new();

    public static PaginatedResponse<T> Ok(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords, string message = "Success.") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            }
        };
}

/// <summary>Pagination metadata included in paginated responses.</summary>
public class PaginationMeta
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
