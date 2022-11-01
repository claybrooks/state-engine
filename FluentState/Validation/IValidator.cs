using FluentState.MachineParts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentState.Validation;

public interface IValidationError
{
    string Reason { get; }
}

public class ValidationError : IValidationError
{
    public string Reason { get; set; } = string.Empty;
}

public interface IValidationWarning
{
    string Reason { get; }
}

public class ValidationWarning : IValidationWarning
{
    public string Reason { get; set; } = string.Empty;
}

public interface IValidationResult
{
    IEnumerable<IValidationError> Errors { get; }
    IEnumerable<IValidationWarning> Warnings { get; }
}

public class ValidationResult : IValidationResult
{
    public IEnumerable<IValidationError> Errors { get; set; } = Array.Empty<IValidationError>();
    public IEnumerable<IValidationWarning> Warnings { get; set; } = Array.Empty<IValidationWarning>();
}

public interface IValidator<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IValidationResult Validate(
        ValidationOptions options,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation);
}

public class ValidationOptions
{
    public bool AllowCycles { get; set; } = true;
    public bool WarnOnAnyCycle { get; set; } = false;
}

internal class TraverseResults<TState>
    where TState : struct
{
    public IReadOnlyList<TState> NonReachableNodes { get; init; } = Array.Empty<TState>();
    public bool IsCyclic { get; init; }
}

public class Validator<TState, TStimulus> : IValidator<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IValidationResult Validate(
        ValidationOptions options,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // Ensure initial state is registered as a state in the state map
        if (!stateMapValidation.HasTopLevelState(initialState))
        {
            errors.Add(new ValidationError
                {Reason = $"Initial state {initialState} is not contained within the StateMap."});
        }

        // List of enum values not registered in the state map
        var states_not_in_state_map = Enum.GetValues(typeof(TState))
            .Cast<TState>()
            .Where(e => !stateMapValidation.HasTopLevelState(e))
            .ToList();

        foreach (var state in states_not_in_state_map)
        {
            warnings.Add(new ValidationWarning{Reason = $"State {state} is not registered with the state map."});
        }

        // Analyze the state machine by traversing
        var traverse_results = DoTraverseStateMachine(initialState, stateMapValidation);
        if (traverse_results.NonReachableNodes.Any())
        {
            errors.Add(new ValidationError{Reason = $"The following states are registered but not reachable: {string.Join(",", traverse_results.NonReachableNodes)}"});
        }

        if (!options.AllowCycles && traverse_results.IsCyclic)
        {
            errors.Add(new ValidationError{Reason = $"There is a cycle in the state machine"});
        }

        if (traverse_results.IsCyclic && options.AllowCycles && options.WarnOnAnyCycle)
        {
            warnings.Add(new ValidationWarning{Reason = $"There is a cycle in the state machine"});
        }

        // Verify that all registered actions have a transition that is registered
        var unreachable_enter_actions = DoFindUnreachableActions(enterRegistryValidation, stateMapValidation);
        var unreachable_leave_actions = DoFindUnreachableActions(leaveRegistryValidation, stateMapValidation);

        if (unreachable_enter_actions.Any())
        {
            errors.Add(new ValidationError{Reason = $"The following transitions can never trigger an action: {string.Join(",", unreachable_enter_actions)}"});
        }

        if (unreachable_leave_actions.Any())
        {
            errors.Add(new ValidationError{Reason = $"The following transitions can never trigger an action: {string.Join(",", unreachable_leave_actions)}"});
        }

        // Verify that all registered guards have a transition that is registered
        var unreachable_guards = DoFindUnreachableGuards(stateGuardValidation, stateMapValidation);

        if (unreachable_guards.Any())
        {
            errors.Add(new ValidationError{Reason = $"The following transitions will never trigger their registered guard: {string.Join(",", unreachable_guards)}"});
        }

        return new ValidationResult
        {
            Errors = errors,
            Warnings = warnings,
        };
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

    private static IReadOnlyList<Transition<TState, TStimulus>> DoFindUnreachableActions(
        IActionRegistryValidation<TState, TStimulus> actionRegistryValidation,
        IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        var registered_transition_triggers = actionRegistryValidation.TransitionActionTransitions;
        
        return registered_transition_triggers.Where(transition => !stateMapValidation.IsTransitionRegistered(transition)).ToList();
    }

    private static IReadOnlyList<Transition<TState, TStimulus>> DoFindUnreachableGuards(
        IStateGuardValidation<TState, TStimulus> guardRegistryValidation,
        IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        var registered_transitions = guardRegistryValidation.GuardTransitions;
        return registered_transitions.Where(transition => !stateMapValidation.IsTransitionRegistered(transition)).ToList();
    }
}