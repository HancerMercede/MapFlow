namespace MapFlow;

/// <summary>
/// Lightweight mapper supporting interface-based, selector-based, and in-place mutation patterns.
/// Zero reflection, zero dependencies, zero runtime overhead.
/// </summary>
public static class Mapper
{
    // ─── Interface-based ────────────────────────────────────────

    /// <summary>
    /// Maps <paramref name="source"/> to a new <typeparamref name="TDestination"/>
    /// using <see cref="IMapFrom{TSource}.MapFrom"/>.
    /// </summary>
    public static TDestination Map<TSource, TDestination>(TSource source)
        where TDestination : IMapFrom<TSource>, new()
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var destination = new TDestination();
        destination.MapFrom(source);
        return destination;
    }

    /// <summary>
    /// Maps each element of <paramref name="source"/> to a new <typeparamref name="TDestination"/>.
    /// </summary>
    public static List<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
        where TDestination : IMapFrom<TSource>, new()
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        return source.Select(Map<TSource, TDestination>).ToList();
    }

    /// <summary>
    /// Maps the items of a <see cref="PagedResult{TSource}"/> preserving pagination metadata.
    /// </summary>
    public static PagedResult<TDestination> Map<TSource, TDestination>(PagedResult<TSource> source)
        where TDestination : IMapFrom<TSource>, new()
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        return new()
        {
            Items = source.Items.Select(Map<TSource, TDestination>).ToList(),
            RowCount = source.RowCount,
            PageCount = source.PageCount,
            PageNumber = source.PageNumber,
            PageSize = source.PageSize
        };
    }

    // ─── Selector-based ─────────────────────────────────────────

    /// <summary>
    /// Projects <paramref name="source"/> using <paramref name="selector"/>.
    /// </summary>
    public static TDestination Map<TSource, TDestination>(
        TSource source,
        Func<TSource, TDestination> selector)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        return selector(source);
    }

    // ─── Apply ──────────────────────────────────────────────────

    /// <summary>
    /// Applies <paramref name="mutator"/> to <paramref name="source"/> and returns the same instance.
    /// </summary>
    public static TSource Apply<TSource>(TSource source, Action<TSource> mutator)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (mutator is null)
            throw new ArgumentNullException(nameof(mutator));

        mutator(source);
        return source;
    }

    /// <summary>
    /// Transforms <paramref name="source"/> via <paramref name="transform"/> and returns the result.
    /// </summary>
    public static TSource Apply<TSource>(TSource source, Func<TSource, TSource> transform)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (transform is null)
            throw new ArgumentNullException(nameof(transform));

        return transform(source);
    }

    /// <summary>
    /// Updates an existing <paramref name="destination"/> from <paramref name="source"/>
    /// using <see cref="IMapFrom{TSource}.MapFrom"/>.
    /// </summary>
    public static void Apply<TSource, TDestination>(TSource source, TDestination destination)
        where TDestination : IMapFrom<TSource>
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (destination is null)
            throw new ArgumentNullException(nameof(destination));

        destination.MapFrom(source);
    }
}
