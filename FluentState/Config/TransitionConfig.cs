using System;
using System.Collections.Generic;

namespace FluentState.Config;

public class TransitionConfig<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public readonly TState State;
    public readonly TStimulus Reason;
    public readonly IList<Action<TState, TState, TStimulus>> EnterActions = new List<Action<TState, TState, TStimulus>>();
    public readonly IList<Action<TState, TState, TStimulus>> LeaveActions = new List<Action<TState, TState, TStimulus>>();
    public readonly IList<Func<TState, TState, TStimulus, bool>> Guards = new List<Func<TState, TState, TStimulus, bool>>();

    public TransitionConfig(TState state, TStimulus reason)
    {
        State = state;
        Reason = reason;
    }
}