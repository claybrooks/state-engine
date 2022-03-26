using System;
using System.Collections.Generic;

namespace FluentState
{
    public class StateMachine<TState, TStimulus> : IStateMachine<TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        private readonly List<Action<TState, TState, TStimulus>> _globalEnterActions = new List<Action<TState, TState, TStimulus>>();
        private readonly List<Action<TState, TState, TStimulus>> _globalLeaveActions = new List<Action<TState, TState, TStimulus>>();
        private readonly Dictionary<TState, IList<Action<TState, TState, TStimulus>>> _stateWideEnterActions = new Dictionary<TState, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<TState, IList<Action<TState, TState, TStimulus>>> _stateWideLeaveActions = new Dictionary<TState, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>> _statePairAndStimulusEnterActions = new Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>> _statePairAndStimulusLeaveActions = new Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<TState, IDictionary<TStimulus,TState>> _stateTransitions = new Dictionary<TState, IDictionary<TStimulus, TState>>();
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, Func<TState, TState, TStimulus, bool>> _stateTransitionGuards = new Dictionary<Tuple<TState, TState, TStimulus>, Func<TState, TState, TStimulus, bool>>();

        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }

        public TState CurrentState { get; private set; }

        public void OverrideState(TState state)
        {
            CurrentState = state;
        }

        public bool AddTransitionGuard(TState fromState, TState toState, TStimulus when, Func<TState, TState, TStimulus, bool> guard)
        {
            var key = Tuple.Create(fromState, toState, when);
            return _stateTransitionGuards.TryAdd(key, guard);
        }

        public bool AddTransition(TState fromState, TState toState, TStimulus when)
        {
            if (!_stateTransitions.ContainsKey(fromState))
            {
                _stateTransitions.Add(fromState, new Dictionary<TStimulus, TState>());
            }

            return _stateTransitions[fromState].TryAdd(when, toState);
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

        public void AddStateLeaveAction(TState leavingState, TState enteringState, TStimulus reason, Action<TState, TState, TStimulus> action)
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

            TriggerStateLeaveActions(CurrentState, nextState, stimulus);
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
            var actionParams = (leavingState, enteringState, reason);

            TriggerActions(_globalEnterActions, actionParams);
            TriggerActions(_stateWideEnterActions, enteringState, actionParams);
            TriggerActions(_statePairAndStimulusEnterActions, actionParams.ToTuple(), actionParams);
        }

        private void TriggerStateLeaveActions(TState leavingState, TState enteringState, TStimulus reason)
        {
            var actionParams = (leavingState, enteringState, reason);

            TriggerActions(_globalLeaveActions, actionParams);
            TriggerActions(_stateWideLeaveActions, leavingState, actionParams);
            TriggerActions(_statePairAndStimulusLeaveActions, actionParams.ToTuple(), actionParams);
        }

        private static void TriggerActions<TKey>(IReadOnlyDictionary<TKey, IList<Action<TState, TState, TStimulus>>> actionMap, TKey key, (TState from, TState to, TStimulus reason) actionParams) 
            where TKey : notnull
        {
            if (actionMap.ContainsKey(key))
            {
                TriggerActions(actionMap[key], actionParams);
            }
        }

        private static void TriggerActions(IEnumerable<Action<TState, TState, TStimulus>> actions, (TState from, TState to, TStimulus reason) actionParams)
        {
            foreach (var action in actions)
            {
                action(actionParams.from, actionParams.to, actionParams.reason);
            }
        }

        #endregion
    }
}