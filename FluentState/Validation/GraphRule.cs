using FluentState.MachineParts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentState.Validation;

internal class TraverseResults<TState>
    where TState : struct
{
    public IReadOnlyList<TState> NonReachableNodes { get; init; } = Array.Empty<TState>();
    public bool IsCyclic { get; init; }
}

public abstract class GraphRule<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private static TraverseResults<TState>? _traverseResults = null;

    internal static TraverseResults<TState> TraverseStateMachine(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        return _traverseResults ??= DoTraverseStateMachine(initialState, stateMapValidation);
    }

    private static TraverseResults<TState> DoTraverseStateMachine(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        var is_cyclic = false;

        // Start with a dictionary of all top-level states
        var visited_states = stateMapValidation.TopLevelStates.ToDictionary(s => s, s => false);
        
        var state_queue = new Queue<TState>();

        visited_states[initialState] = true;
        state_queue.Enqueue(initialState);

        while (state_queue.Count > 0)
        {
            var state = state_queue.Dequeue();
            var next_states = stateMapValidation.StateTransitions(state).Values.ToList();
            foreach (var next_state in next_states)
            {
                if (!visited_states.ContainsKey(next_state) || visited_states[next_state] == false)
                {
                    visited_states[next_state] = true;
                    state_queue.Enqueue(next_state);
                }
                else
                {
                    is_cyclic = true;
                }
            }
        }

        return new TraverseResults<TState>
        {
            NonReachableNodes = visited_states.Where(kvp => kvp.Value == false).Select(kvp => kvp.Key).ToList(),
            IsCyclic = is_cyclic
        };
    }

}

