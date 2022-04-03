using FluentState.Config;
using FluentState.Machine;
using System;

namespace FluentState.Builder
{
    public class TStateMachineBuilder<TStateMachine, TState, TStimulus> : IStateMachineBuilder<TStateMachine, TState, TStimulus>
        where TStateMachine : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        protected readonly TStateMachine _machine;

        public TStateMachineBuilder(TState initialState)
        {
            var maybeMachine = (TStateMachine)Activator.CreateInstance(typeof(TStateMachine), initialState)!;
            if (maybeMachine == null)
            {
                throw new InvalidOperationException("Unable to create state machine");
            }
            _machine = maybeMachine;
        }

        public IStateMachine<TState, TStimulus> Machine => _machine;

        public IStateBuilder<TStateMachine, TState, TStimulus> WithState(TState from)
        {
            return new StateBuilder<TStateMachine, TState, TStimulus>(this, from);
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithEnterAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateEnterAction(action);
            return this;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _machine.AddStateLeaveAction(action);
            return this;
        }

        public TStateMachine Build()
        {
            return _machine;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithUnboundedHistory()
        {
            _machine.History.Enabled = true;
            _machine.History.MakeUnbounded();
            return this;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithBoundedHistory(int size)
        {
            _machine.History.Enabled = true;
            _machine.History.MakeBounded(size);
            return this;
        }

        public IStateMachineBuilder<TStateMachine, TState, TStimulus> WithConfig(IConfigLoader<TState, TStimulus> loader)
        {
            Load(loader);
            return this;
        }

        #region Private Config Building

        private void Load(IConfigLoader<TState, TStimulus> loader)
        {
            Machine.OverrideState(loader.InitialState);
            LoadGlobalActions(loader);
            LoadStates(loader);
        }

        private void LoadGlobalActions(IConfigLoader<TState, TStimulus> loader)
        {
            foreach (var enterAction in loader.GlobalEnterActions)
            {
                WithEnterAction(enterAction);
            }

            foreach (var leaveAction in loader.GlobalLeaveActions)
            {
                WithLeaveAction(leaveAction);
            }
        }

        private void LoadStates(IConfigLoader<TState, TStimulus> loader)
        {
            foreach (var stateConfig in loader.States)
            {
                LoadState(stateConfig);
            }
        }

        private void LoadState(StateConfig<TState, TStimulus> stateConfig)
        {
            var stateBuilder = WithState(stateConfig.State);

            foreach (var enterAction in stateConfig.EnterActions)
            {
                stateBuilder.WithEnterAction(enterAction);
            }

            foreach (var leaveAction in stateConfig.LeaveActions)
            {
                stateBuilder.WithLeaveAction(leaveAction);
            }

            foreach (var transition in stateConfig.Transitions)
            {
                stateBuilder.WithTransitionTo(
                   transition.State,
                   transition.Reason,
                   transition.Guards,
                   transition.EnterActions,
                   transition.LeaveActions
               );
            }
        }

        #endregion
    }
}
