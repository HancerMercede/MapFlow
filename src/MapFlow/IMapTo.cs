namespace MapFlow;

/// <summary>
/// Defines a mapping from the implementing type to <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TDestination">Target type to map to.</typeparam>
/// <example>
/// public class User : IMapTo&lt;UserDto&gt; { ... }
/// </example>
public interface IMapTo<out TDestination>
{
    /// <summary>
    /// Creates a new <typeparamref name="TDestination"/> from this instance.
    /// </summary>
    TDestination MapTo();
}
