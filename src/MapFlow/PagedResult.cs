namespace MapFlow;

/// <summary>
/// Represents a paginated result set with pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public class PagedResult<T>
{
    /// <summary>Total number of items across all pages.</summary>
    public int RowCount { get; set; }

    /// <summary>Current page number (1-based).</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Total number of pages.</summary>
    public int PageCount { get; set; }

    /// <summary>Items for the current page.</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// Projects the items of this page using <paramref name="selector"/>,
    /// preserving pagination metadata.
    /// </summary>
    /// <example>
    /// paged.Map(x =&gt; x.ToString());
    /// </example>
    public PagedResult<TDest> Map<TDest>(Func<T, TDest> selector) => new()
    {
        Items = Items.Select(selector).ToList(),
        RowCount = RowCount,
        PageCount = PageCount,
        PageNumber = PageNumber,
        PageSize = PageSize
    };
}
