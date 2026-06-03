namespace MapFlow;

/// <summary>
/// Fluent extension methods for <see cref="Mapper"/>.
/// </summary>
public static class MapperExtensions
{
    // ─── Interface-based ────────────────────────────────────────

    /// <summary>
    /// Maps this instance to a new <typeparamref name="TDestination"/>.
    /// </summary>
    public static TDestination MapTo<TSource, TDestination>(this TSource source)
        where TDestination : IMapFrom<TSource>, new()
        => Mapper.Map<TSource, TDestination>(source);

    /// <summary>
    /// Maps each element to a new <typeparamref name="TDestination"/>.
    /// </summary>
    public static IEnumerable<TDestination> MapTo<TSource, TDestination>(this IEnumerable<TSource> source)
        where TDestination : IMapFrom<TSource>, new()
        => Mapper.Map<TSource, TDestination>(source);

    // ─── Selector-based ─────────────────────────────────────────

    /// <summary>
    /// Maps this instance using <paramref name="selector"/>.
    /// </summary>
    public static TDestination Map<TSource, TDestination>(
        this TSource source,
        Func<TSource, TDestination> selector)
    {
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        return selector(source);
    }

    /// <summary>
    /// Maps each element using <paramref name="selector"/>.
    /// </summary>
    public static List<TDestination> Map<TSource, TDestination>(
        this IEnumerable<TSource> source,
        Func<TSource, TDestination> selector)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        return source.Select(selector).ToList();
    }

    // ─── Apply ──────────────────────────────────────────────────

    /// <summary>
    /// Applies <paramref name="mutator"/> and returns the same instance.
    /// </summary>
    public static TSource Apply<TSource>(this TSource source, Action<TSource> mutator)
        => Mapper.Apply(source, mutator);

    /// <summary>
    /// Transforms this instance and returns the result.
    /// </summary>
    public static TSource Apply<TSource>(this TSource source, Func<TSource, TSource> transform)
        => Mapper.Apply(source, transform);
}
