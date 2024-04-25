namespace StateEngine;

public interface ITransition<out TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    TState From { get; }
    TState To { get; }
    TStimulus? Reason { get; }
}

public interface ITransitionComparer<in TState, TStimulus> : IEqualityComparer<ITransition<TState, TStimulus>>
    where TState : struct
    where TStimulus : struct
{
}

public sealed class Transition<TState, TStimulus> : ITransition<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public TState From { get; set; }
    public TState To { get; set; }
    public TStimulus? Reason { get; set; }

    public override string ToString()
    {
        return $"From: {From}, To: {To}, Reason: {Reason}";
    }
}

public sealed class TransitionComparer<TState, TStimulus> : ITransitionComparer<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public bool Equals(ITransition<TState, TStimulus>? x, ITransition<TState, TStimulus>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.From.Equals(y.From) && x.To.Equals(y.To) && x.Reason.Equals(y.Reason);
    }

    public int GetHashCode(ITransition<TState, TStimulus> obj)
    {
#if ! NETSTANDARD2_1_OR_GREATER
        return new {obj.From, obj.To, obj.Reason}.GetHashCode();
#else
        return HashCode.Combine(obj.From, obj.To, obj.Reason);
#endif
    }
}
