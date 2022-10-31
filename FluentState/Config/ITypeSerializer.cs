namespace FluentState.Config;

public interface ITypeSerializer<TType>
    where TType : struct
{
    /// <summary>
    /// Convert a stringified <typeparamref name="TType"/> value to a <typeparamref name="TType"/>
    /// </summary>
    /// <remarks>
    /// If the provided string is invalid, implementations should return null.
    /// It is up to the implementations to handle changes to serialized formats over time.
    /// </remarks>
    /// <param name="state"></param>
    /// <returns></returns>
    TType? Convert(string state);


    /// <summary>
    /// Convert a <typeparamref name="TType"/> to it's stringified version
    /// </summary>
    /// <remarks>
    /// Implementations should do their best to never return null.
    /// </remarks>
    /// <param name="state"></param>
    /// <returns></returns>
    string Convert(TType state);
}