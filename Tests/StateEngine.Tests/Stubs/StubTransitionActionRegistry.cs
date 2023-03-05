namespace StateEngine.Tests.Stubs;

public class StubTransitionActionRegistry<TState, TStimulus> : ITransitionActionRegistry<TState, TStimulus>, ITransitionActionRegistryValidation<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    public bool AnythingRegistered => GlobalActionRegistered || StateActionRegistered || TransitionActionRegistered;
    public bool GlobalActionRegistered = false;
    public bool StateActionRegistered = false;
    public bool TransitionActionRegistered = false;

    public bool TriggerInvoked = false;

    public void Register(ITransitionAction<TState, TStimulus> transitionAction)
    {
        GlobalActionRegistered = true;
    }

    public void Register(TState state, ITransitionAction<TState, TStimulus> transitionAction)
    {
        StateActionRegistered = true;
    }

    public void Register(TState from, TState to, TStimulus when, ITransitionAction<TState, TStimulus> transitionAction)
    {
        TransitionActionRegistered = true;
    }

    public void Register(ITransition<TState, TStimulus> transition, ITransitionAction<TState, TStimulus> transitionAction)
    {
        TransitionActionRegistered = true;
    }

    public void Trigger(TState state, ITransition<TState, TStimulus> transition)
    {
        TriggerInvoked = true;
    }

    public IEnumerable<string> GlobalActions { get; } = new List<string>();

    public IReadOnlyDictionary<TState, IEnumerable<string>> StateWideActions { get; } =
        new Dictionary<TState, IEnumerable<string>>();

    public IReadOnlyDictionary<ITransition<TState, TStimulus>, IEnumerable<string>> ActionsOnTransition { get; } =
        new Dictionary<ITransition<TState, TStimulus>, IEnumerable<string>>();
}