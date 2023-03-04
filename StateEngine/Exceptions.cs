namespace StateEngine;

public class UnregisteredTransitionException : Exception
{
    public UnregisteredTransitionException(string message) : base(message)
    {

    }
}

public class UnregisteredTransitionException<TState, TStimulus> : UnregisteredTransitionException
where TState : struct
where TStimulus : struct
{
    public TState State { get; }
    public TStimulus Stimulus { get; }

    public UnregisteredTransitionException(TState state, TStimulus stimulus) : base($"No available state transition from {state} with stimulus {stimulus}")
    {
        State = state;
        Stimulus = stimulus;
    }
}

public class TransitioningToCurrentStateException : Exception
{
    public TransitioningToCurrentStateException(string message) : base(message)
    {

    }
}

public class TransitioningToCurrentStateException<TState, TStimulus> : TransitioningToCurrentStateException
    where TState : struct
    where TStimulus : struct
{
    public TState State { get; }
    public TStimulus Stimulus { get; }

    public TransitioningToCurrentStateException(TState state, TStimulus stimulus) : base($"Trying to transition to {state} while in that state with stimulus {stimulus}")
    {
        State = state;
        Stimulus = stimulus;
    }
}