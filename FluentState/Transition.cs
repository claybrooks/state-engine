using System;
using System.Collections.Generic;

namespace FluentState;

public interface ITransition<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    TState From { get; init; }
    TState To { get; init; }
    TStimulus Reason { get; init; }
}

public interface ITransitionComparer<TState, TStimulus> : IEqualityComparer<ITransition<TState, TStimulus>>
    where TState : struct
    where TStimulus : struct
{
}

internal sealed class Transition<TState, TStimulus> : ITransition<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public TState From { get; init; }
    public TState To { get; init; }
    public TStimulus Reason { get; init; }

    public override string ToString()
    {
        return $"From: {From}, To: {To}, Reason: {Reason}";
    }
}

internal sealed class TransitionComparer<TState, TStimulus> : ITransitionComparer<TState, TStimulus>
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
        return HashCode.Combine(obj.From, obj.To, obj.Reason);
    }
}
