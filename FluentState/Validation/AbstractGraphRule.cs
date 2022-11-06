using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentState;

public class TraverseResult<TState>
    where TState : struct
{
    public IReadOnlyList<TState> NonReachableNodes { get; init; } = Array.Empty<TState>();
    public bool IsCyclic { get; init; }
}

public interface IGraphRule<TState, TStimulus> : IValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    TraverseResult<TState> TraverseStateMachine(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation);
}

/// <summary>
///  Static TraverseResult internally so only one traversal of the graph is done across many graph rules
///  This means TraverseResult is now the 
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TStimulus"></typeparam>
public abstract class AbstractGraphRule<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>, IGraphRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private static TraverseResult<TState>? _traverseResults;

    public TraverseResult<TState> TraverseStateMachine(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        return _traverseResults ??= DoTraverseStateMachine(initialState, stateMapValidation);
    }

    private static TraverseResult<TState> DoTraverseStateMachine(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation)
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

        return new TraverseResult<TState>
        {
            NonReachableNodes = visited_states.Where(kvp => kvp.Value == false).Select(kvp => kvp.Key).ToList(),
            IsCyclic = is_cyclic
        };
    }
}
