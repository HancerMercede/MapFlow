namespace MapFlow;

/// <summary>
/// Defines a mapping from the implementing type to <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TDestination">Target type to map to.</typeparam>
public interface IMapTo<out TDestination>
{
    /// <summary>
    /// Creates a new <typeparamref name="TDestination"/> from this instance.
    /// </summary>
    TDestination MapTo();
}
