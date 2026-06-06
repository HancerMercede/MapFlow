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
    /// <example>
    /// entity.MapTo&lt;User, UserDto&gt;();
    /// </example>
    public static TDestination MapTo<TSource, TDestination>(this TSource source)
        where TDestination : IMapFrom<TSource>, new()
        => Mapper.Map<TSource, TDestination>(source);

    /// <summary>
    /// Maps each element to a new <typeparamref name="TDestination"/>.
    /// </summary>
    /// <example>
    /// users.MapTo&lt;User, UserDto&gt;();
    /// </example>
    public static IEnumerable<TDestination> MapTo<TSource, TDestination>(this IEnumerable<TSource> source)
        where TDestination : IMapFrom<TSource>, new()
        => Mapper.Map<TSource, TDestination>(source);

    /// <summary>
    /// Maps the items of a <see cref="PagedResult{TSource}"/> to <typeparamref name="TDestination"/>,
    /// preserving pagination metadata.
    /// </summary>
    /// <example>
    /// clients.MapTo&lt;Client, ClientDto&gt;();
    /// </example>
    public static PagedResult<TDestination> MapTo<TSource, TDestination>(this PagedResult<TSource> source)
        where TDestination : IMapFrom<TSource>, new()
        => Mapper.Map<TSource, TDestination>(source);

    // ─── Project (IMapTo) ────────────────────────────────────────

    /// <summary>
    /// Projects this instance to a new <typeparamref name="TDestination"/> via <see cref="IMapTo{TDestination}"/>.
    /// </summary>
    /// <example>
    /// entity.Project&lt;UserDto&gt;();
    /// </example>
    public static TDestination Project<TDestination>(this IMapTo<TDestination> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.MapTo();
    }

    // ─── Selector-based ─────────────────────────────────────────

    /// <summary>
    /// Maps this instance using <paramref name="selector"/>.
    /// </summary>
    /// <example>
    /// user.Map(e =&gt; new UserDto { ... });
    /// </example>
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
    /// <example>
    /// users.Map(e =&gt; new UserDto { ... });
    /// </example>
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
    /// <example>
    /// user.Apply(u =&gt; u.Name = "new");
    /// </example>
    public static TSource Apply<TSource>(this TSource source, Action<TSource> mutator)
        => Mapper.Apply(source, mutator);

    /// <summary>
    /// Transforms this instance and returns the result.
    /// </summary>
    /// <example>
    /// user.Apply(u =&gt; new User(u.Name.ToUpper()));
    /// </example>
    public static TSource Apply<TSource>(this TSource source, Func<TSource, TSource> transform)
        => Mapper.Apply(source, transform);
}
