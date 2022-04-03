
using FluentState.Persistence;

namespace Tester
{
    public enum State
    {
        Invalid,
        Idle,
        Walking,
        Running,
        Crouched,
        CrouchWalking,
    }

    public enum Stimulus
    {
        QuickStop,
        Stop,
        Walk,
        Run,
        Crouch,
    }

    public class StateTypeConverter : ITypeSerializer<State>
    {
        public State? Convert(string stateString)
        {
            if (Enum.TryParse(stateString, out State state))
            {
                return state;
            }
            return null;
        }
        public string Convert(State state)
        {
            return state.ToString();
        }
    }
    public class StimulusTypeConverter : ITypeSerializer<Stimulus>
    {
        public Stimulus? Convert(string stimulusString)
        {
            if (Enum.TryParse(stimulusString, out Stimulus state))
            {
                return state;
            }
            return null;
        }
        public string Convert(Stimulus state)
        {
            return state.ToString();
        }
    }

    public class ActionProvider : FluentState.Config.IActionProvider<State, Stimulus>
    {
        public readonly Dictionary<string, Action<State, State, Stimulus>> Actions = new Dictionary<string, Action<State, State, Stimulus>>();
        public Action<State, State, Stimulus> Get(string key)
        {
            return Actions[key];
        }
    }

    public class GuardProvider : FluentState.Config.IGuardProvider<State, Stimulus>
    {
        public readonly Dictionary<string, Func<State, State, Stimulus, bool>> Guards = new Dictionary<string, Func<State, State, Stimulus, bool>>();
        public Func<State, State, Stimulus, bool> Get(string key)
        {
            return Guards[key];
        }
    }

}
