
using FluentState.Persistence;

namespace Tester
{
    public enum State
    {
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

    public class StateTypeConverter : IStateTypeConverter<State>
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
}
