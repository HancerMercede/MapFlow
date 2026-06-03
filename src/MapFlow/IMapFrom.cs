namespace MapFlow;

/// <summary>
/// Defines a mapping from <typeparamref name="TSource"/> to the implementing type.
/// The implementing type is responsible for populating its own properties.
/// </summary>
/// <typeparam name="TSource">Source type to map from.</typeparam>
public interface IMapFrom<in TSource>
{
    /// <summary>
    /// Populates this instance with data from <paramref name="source"/>.
    /// </summary>
    void MapFrom(TSource source);
}
