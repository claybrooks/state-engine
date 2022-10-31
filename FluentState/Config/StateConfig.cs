using System;
using System.Collections.Generic;

namespace FluentState.Config;

public class StateConfig<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public readonly TState State;
    public readonly IList<Action<TState, TState, TStimulus>> EnterActions = new List<Action<TState, TState, TStimulus>>();
    public readonly IList<Action<TState, TState, TStimulus>> LeaveActions = new List<Action<TState, TState, TStimulus>>();
    public readonly IList<TransitionConfig<TState, TStimulus>> Transitions = new List<TransitionConfig<TState, TStimulus>>();

    public StateConfig(TState state)
    {
        State = state;
    }
}