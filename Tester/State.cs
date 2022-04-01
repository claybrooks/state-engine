﻿
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
}
