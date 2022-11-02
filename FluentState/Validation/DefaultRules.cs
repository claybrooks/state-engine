using FluentState.MachineParts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentState.Validation;

public static class DefaultRules
{
    public static List<IValidationRule<TState, TStimulus>> Get<TState, TStimulus>() 
        where TState : struct
        where TStimulus : struct
    {
        return new List<IValidationRule<TState, TStimulus>>
        {
            new InitialStateIsRegistered<TState, TStimulus>(),
            new UnregisteredEnumValues<TState, TStimulus>(),
            new UnreachableStates<TState, TStimulus>(),
            new UnreachableAction<TState, TStimulus>(),
            new UnreachableGuard<TState, TStimulus>()
        };
    }

    public static List<IValidationRule<TState, TStimulus>> Get<TState, TStimulus>(IEnumerable<IValidationRule<TState, TStimulus>> extraRules) 
        where TState : struct
        where TStimulus : struct
    {
        var rules = Get<TState, TStimulus>();
        rules.AddRange(extraRules);
        return rules;
    }
}

public class InitialStateIsRegistered<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        if (!stateMapValidation.HasTopLevelState(initialState))
        {
            Errors.Add(new ValidationError
                {Reason = $"Initial state {initialState} is not contained within the StateMap."});
        }

        return Result;
    }
}

public class UnregisteredEnumValues<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        var states_not_in_state_map = Enum.GetValues(typeof(TState))
            .Cast<TState>()
            .Where(e => !stateMapValidation.HasTopLevelState(e))
            .ToList();

        foreach (var state in states_not_in_state_map)
        {
            Warnings.Add(new ValidationWarning{Reason = $"State {state} is not registered with the state map."});
        }

        return Result;
    }
}

public class UnreachableStates<TState, TStimulus> : GraphRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        var traverse_results = TraverseStateMachine(initialState, stateMapValidation);
        if (traverse_results.NonReachableNodes.Any())
        {
            Errors.Add(new ValidationError{Reason = $"The following states are registered but not reachable: {string.Join(",", traverse_results.NonReachableNodes)}"});
        }

        return Result;
    }
}

public class NoCycles<TState, TStimulus> : GraphRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        var traverse_results = TraverseStateMachine(initialState, stateMapValidation);
        if (traverse_results.IsCyclic)
        {
            Errors.Add(new ValidationError{Reason = $"There is a cycle in the state machine"});
        }

        return Result;
    }
}

public class UnreachableAction<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        var unreachable_enter_actions = DoFindUnreachableActions(enterRegistryValidation, stateMapValidation);
        var unreachable_leave_actions = DoFindUnreachableActions(leaveRegistryValidation, stateMapValidation);

        if (unreachable_enter_actions.Any())
        {
            Errors.Add(new ValidationError{Reason = $"The following transitions can never trigger an action: {string.Join(",", unreachable_enter_actions)}"});
        }

        if (unreachable_leave_actions.Any())
        {
            Errors.Add(new ValidationError{Reason = $"The following transitions can never trigger an action: {string.Join(",", unreachable_leave_actions)}"});
        }

        return Result;
    }

    
    private static IReadOnlyList<Transition<TState, TStimulus>> DoFindUnreachableActions(
        IActionRegistryValidation<TState, TStimulus> actionRegistryValidation,
        IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        var registered_transition_triggers = actionRegistryValidation.TransitionActionTransitions;
        
        return registered_transition_triggers.Where(transition => !stateMapValidation.IsTransitionRegistered(transition)).ToList();
    }
}

public class UnreachableGuard<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IStateGuardValidation<TState, TStimulus> stateGuardValidation)
    {
        var unreachable_guards = DoFindUnreachableGuards(stateGuardValidation, stateMapValidation);

        if (unreachable_guards.Any())
        {
            Errors.Add(new ValidationError{Reason = $"The following transitions will never trigger their registered guard: {string.Join(",", unreachable_guards)}"});
        }

        return Result;
    }

    private static IReadOnlyList<Transition<TState, TStimulus>> DoFindUnreachableGuards(
        IStateGuardValidation<TState, TStimulus> guardRegistryValidation,
        IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        var registered_transitions = guardRegistryValidation.GuardTransitions;
        return registered_transitions.Where(transition => !stateMapValidation.IsTransitionRegistered(transition)).ToList();
    }
}