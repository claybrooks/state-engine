using FluentState.History;
using System;
using System.Collections.Generic;

namespace FluentState
{
    public class StateMachine<TState, TStimulus> : IStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        // Actions to be run whenever any state is entered
        private readonly List<Action<TState, TState, TStimulus>> _globalEnterActions = new List<Action<TState, TState, TStimulus>>();

        // Actions to be run whenever any state is left
        private readonly List<Action<TState, TState, TStimulus>> _globalLeaveActions = new List<Action<TState, TState, TStimulus>>();

        // Actions to be run whenever a specific state is entered, regardless of the previous state or why the transition happened
        private readonly Dictionary<TState, IList<Action<TState, TState, TStimulus>>> _stateWideEnterActions = new Dictionary<TState, IList<Action<TState, TState, TStimulus>>>();

        // Actions to be run whenever a specific state is left, regardless of the previous state or why the transition happened
        private readonly Dictionary<TState, IList<Action<TState, TState, TStimulus>>> _stateWideLeaveActions = new Dictionary<TState, IList<Action<TState, TState, TStimulus>>>();

        // Actions to be run whenever a specific state is entered, depending on it's previous state and why the transition happened
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>> _statePairAndStimulusEnterActions = new Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>>();

        // Actions to be run whenever a specific state is left, depending on it's next state and why the transition happened
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>> _statePairAndStimulusLeaveActions = new Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>>();

        // All of the allowable state transition pairs, coupled with the stimulus that triggers the transition
        private readonly Dictionary<TState, IDictionary<TStimulus, TState>> _stateTransitions = new Dictionary<TState, IDictionary<TStimulus, TState>>();

        // Guards to be invoked whenever a state transition happens for a specific reason
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, Func<TState, TState, TStimulus, bool>> _stateTransitionGuards = new Dictionary<Tuple<TState, TState, TStimulus>, Func<TState, TState, TStimulus, bool>>();

        // Optional history tracking of this state machine
        private readonly IStateMachineHistory<TState, TStimulus> _history = new StateMachineHistory<TState, TStimulus>();

        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }

        public TState CurrentState { get; private set; }

        public IStateMachineHistory<TState, TStimulus> History => _history;

        public void OverrideState(TState state)
        {
            CurrentState = state;
        }

        public bool AddTransitionGuard(TState enteringState, TState leavingState, TStimulus when, Func<TState, TState, TStimulus, bool> guard)
        {
            var key = Tuple.Create(leavingState, enteringState, when);
            return _stateTransitionGuards.TryAdd(key, guard);
        }

        public bool AddTransition(TState enteringState, TState leavingState, TStimulus when)
        {
            if (!_stateTransitions.ContainsKey(leavingState))
            {
                _stateTransitions.Add(leavingState, new Dictionary<TStimulus, TState>());
            }

            return _stateTransitions[leavingState].TryAdd(when, enteringState);
        }

        public void AddStateEnterAction(Action<TState, TState, TStimulus> action)
        {
            _globalEnterActions.Add(action);
        }

        public void AddStateEnterAction(TState state, Action<TState, TState, TStimulus> action)
        {
            if (!_stateWideEnterActions.ContainsKey(state))
            {
                _stateWideEnterActions.Add(state, new List<Action<TState, TState, TStimulus>>());
            }
            _stateWideEnterActions[state].Add(action);
        }

        public void AddStateEnterAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            var key = Tuple.Create(enteringState, leavingState, reason);

            if (!_statePairAndStimulusEnterActions.ContainsKey(key))
            {
                _statePairAndStimulusEnterActions.Add(key, new List<Action<TState, TState, TStimulus>>());
            }
            _statePairAndStimulusEnterActions[key].Add(action);
        }

        public void AddStateLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _globalLeaveActions.Add(action);
        }

        public void AddStateLeaveAction(TState state, Action<TState, TState, TStimulus> action)
        {
            if (!_stateWideLeaveActions.ContainsKey(state))
            {
                _stateWideLeaveActions.Add(state, new List<Action<TState, TState, TStimulus>>());
            }
            _stateWideLeaveActions[state].Add(action);
        }

        public void AddStateLeaveAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            var key = Tuple.Create(leavingState, enteringState, reason);

            if (!_statePairAndStimulusLeaveActions.ContainsKey(key))
            {
                _statePairAndStimulusLeaveActions.Add(key, new List<Action<TState, TState, TStimulus>>());
            }
            _statePairAndStimulusLeaveActions[key].Add(action);
        }

        public bool Post(TStimulus stimulus)
        {
            // Unable to get the next state with the supplied stimulus
            if (!TryGetNextState(CurrentState, stimulus, out TState nextState))
            {
                return false;
            }

            // The next state is the current state, so not transition
            if (CurrentState.Equals(nextState))
            {
                return false;
            }

            if (!PassesGuard(CurrentState, nextState, stimulus))
            {
                return false;
            }

            _history.Add(nextState, CurrentState, stimulus);

            TriggerStateLeaveActions(nextState, CurrentState, stimulus);
            TState previousState = CurrentState;
            CurrentState = nextState;
            TriggerStateEnterActions(nextState, previousState, stimulus);

            return true;
        }

        #region Protected

        protected bool PassesGuard(TState from, TState to, TStimulus reason)
        {
            var guardKey = Tuple.Create(from, to, reason);
            if (!_stateTransitionGuards.ContainsKey(guardKey))
            {
                return true;
            }

            return _stateTransitionGuards[guardKey](from, to, reason);
        }
        #endregion

        #region Private

        private bool TryGetNextState(TState currentState, TStimulus stimulus, out TState nextState)
        {
            nextState = currentState;

            if (!_stateTransitions.ContainsKey(currentState))
            {
                return false;
            }

            if (!_stateTransitions[currentState].ContainsKey(stimulus))
            {
                return false;
            }

            nextState = _stateTransitions[currentState][stimulus];
            return true;
        }

        private void TriggerStateEnterActions(TState enteringState, TState leavingState, TStimulus reason)
        {
            var actionParams = (enteringState, leavingState, reason);

            TriggerActions(_globalEnterActions, actionParams);
            TriggerActions(_stateWideEnterActions, enteringState, actionParams);
            TriggerActions(_statePairAndStimulusEnterActions, actionParams.ToTuple(), actionParams);
        }

        private void TriggerStateLeaveActions(TState enteringState, TState leavingState, TStimulus reason)
        {
            var actionParams = (enteringState, leavingState, reason);

            TriggerActions(_globalLeaveActions, actionParams);
            TriggerActions(_stateWideLeaveActions, leavingState, actionParams);
            TriggerActions(_statePairAndStimulusLeaveActions, actionParams.ToTuple(), actionParams);
        }

        private static void TriggerActions<TKey>(IReadOnlyDictionary<TKey, IList<Action<TState, TState, TStimulus>>> actionMap, TKey key, (TState enteringState, TState leavingState, TStimulus reason) actionParams)
            where TKey : notnull
        {
            if (actionMap.ContainsKey(key))
            {
                TriggerActions(actionMap[key], actionParams);
            }
        }

        private static void TriggerActions(IEnumerable<Action<TState, TState, TStimulus>> actions, (TState enteringState, TState leavingState, TStimulus reason) actionParams)
        {
            foreach (var action in actions)
            {
                action(actionParams.enteringState, actionParams.leavingState, actionParams.reason);
            }
        }

        #endregion
    }
}