using System;
using System.Collections.Generic;

namespace FluentState;

public class Transition<TState, TStimulus> where TState : struct where TStimulus : struct
{
    public TState From { get; init; }
    public TState To { get; init; }
    public TStimulus Reason { get; init; }

    public override string ToString()
    {
        return $"From: {From}, To: {To}, Reason: {Reason}";
    }
}

public class TransitionComparer<TState, TStimulus> : IEqualityComparer<Transition<TState, TStimulus>> where TState : struct where TStimulus : struct
{
    public bool Equals(Transition<TState, TStimulus>? x, Transition<TState, TStimulus>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.From.Equals(y.From) && x.To.Equals(y.To) && x.Reason.Equals(y.Reason);
    }

    public int GetHashCode(Transition<TState, TStimulus> obj)
    {
        return HashCode.Combine(obj.From, obj.To, obj.Reason);
    }
}