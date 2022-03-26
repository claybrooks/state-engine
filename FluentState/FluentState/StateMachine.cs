namespace FluentState
{
    public class StateMachine<TState, TStimulus> 
        where TState : notnull
        where TStimulus : notnull
    {
        private readonly List<Action<TState, TState, TStimulus>> _allEnterActions = new List<Action<TState, TState, TStimulus>>();
        private readonly List<Action<TState, TState, TStimulus>> _allLeaveActions = new List<Action<TState, TState, TStimulus>>();
        private readonly Dictionary<TState, IList<Action<TState, TState, TStimulus>>> _stateEnterActions = new Dictionary<TState, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<TState, IList<Action<TState, TState, TStimulus>>> _stateLeaveActions = new Dictionary<TState, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>> _stateStimulusEnterActions = new Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>> _stateStimulusLeaveActions = new Dictionary<Tuple<TState, TState, TStimulus>, IList<Action<TState, TState, TStimulus>>>();
        private readonly Dictionary<TState, IDictionary<TStimulus,TState>> _stateTransitions = new Dictionary<TState, IDictionary<TStimulus, TState>>();

        public StateMachine(TState initialState)
        {
            CurrentState = initialState;
        }

        public TState CurrentState { get; private set; }

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
            _allEnterActions.Add(action);
        }

        public void AddStateEnterAction(TState state, Action<TState, TState, TStimulus> action)
        {
            if (!_stateEnterActions.ContainsKey(state))
            {
                _stateEnterActions.Add(state, new List<Action<TState, TState, TStimulus>>());
            }
            _stateEnterActions[state].Add(action);
        }

        public void AddStateEnterAction(TState enteringState, TState leavingState, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            var key = Tuple.Create(enteringState, leavingState, reason);

            if (!_stateStimulusEnterActions.ContainsKey(key))
            {
                _stateStimulusEnterActions.Add(key, new List<Action<TState, TState, TStimulus>>());
            }
            _stateStimulusEnterActions[key].Add(action);
        }

        public void AddStateLeaveAction(Action<TState, TState, TStimulus> action)
        {
            _allLeaveActions.Add(action);
        }

        public void AddStateLeaveAction(TState state, Action<TState, TState, TStimulus> action)
        {
            if (!_stateLeaveActions.ContainsKey(state))
            {
                _stateLeaveActions.Add(state, new List<Action<TState, TState, TStimulus>>());
            }
            _stateLeaveActions[state].Add(action);
        }

        public void AddStateLeaveAction(TState leavingState, TState enteringState, TStimulus reason, Action<TState, TState, TStimulus> action)
        {
            var key = Tuple.Create(leavingState, enteringState, reason);

            if (!_stateStimulusLeaveActions.ContainsKey(key))
            {
                _stateStimulusLeaveActions.Add(key, new List<Action<TState, TState, TStimulus>>());
            }
            _stateStimulusLeaveActions[key].Add(action);
        }

        public bool Poke(TStimulus stimulus)
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

            TriggerStateLeaveActions(CurrentState, nextState, stimulus);
            TState previousState = CurrentState;
            CurrentState = nextState;
            TriggerStateEnterActions(nextState, previousState, stimulus);

            return true;
        }

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

            TriggerActions(_allEnterActions, actionParams);
            TriggerActions(_stateEnterActions, enteringState, actionParams);
            TriggerActions(_stateStimulusEnterActions, actionParams.ToTuple(), actionParams);
        }

        private void TriggerStateLeaveActions(TState leavingState, TState enteringState, TStimulus reason)
        {
            var actionParams = (enteringState, leavingState, reason);

            TriggerActions(_allLeaveActions, actionParams);
            TriggerActions(_stateLeaveActions, leavingState, actionParams);
            TriggerActions(_stateStimulusLeaveActions, actionParams.ToTuple(), actionParams);
        }

        private void TriggerActions<TKey>(IReadOnlyDictionary<TKey, IList<Action<TState, TState, TStimulus>>> actionMap, TKey key, (TState enteringState, TState leavingState, TStimulus reason) actionParams)
        {
            if (actionMap.ContainsKey(key))
            {
                TriggerActions(actionMap[key], actionParams);
            }
        }

        private void TriggerActions(IEnumerable<Action<TState, TState, TStimulus>> actions, (TState enteringState, TState leavingState, TStimulus reason) actionParams)
        {
            foreach (var action in actions)
            {
                action(actionParams.enteringState, actionParams.leavingState, actionParams.reason);
            }
        }

        #endregion
    }
}