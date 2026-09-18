namespace JobApplicationManagement.Application.Common.Models;

/// <summary>
/// Reusable pagination wrapper for any paginated query result.
/// </summary>
public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
