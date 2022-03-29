namespace FluentState.Persistence
{
    public interface IStateTypeConverter<TState>
        where TState : struct
    {
        TState? Convert(string state);
        string Convert(TState state);
    }
}
