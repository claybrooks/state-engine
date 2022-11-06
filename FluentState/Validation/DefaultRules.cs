using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentState;

public static class DefaultRules
{
    public static List<IValidationRule<TState, TStimulus>> Get<TState, TStimulus>() 
        where TState : struct
        where TStimulus : struct
    {
        return new List<IValidationRule<TState, TStimulus>>
        {
            new InitialStateIsUnregistered<TState, TStimulus>(),
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

public sealed class InitialStateIsUnregistered<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        if (!stateMapValidation.HasTopLevelState(initialState))
        {
            Errors.Add(new ValidationError<TState, TStimulus>
                {
                    Reason = $"Initial state {initialState} is not contained within the StateMap.",
                    ErrorStates = new List<TState> {initialState}
                });
            
        }

        return Result;
    }
}

public sealed class UnregisteredEnumValues<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        var states_not_in_state_map = Enum.GetValues(typeof(TState))
            .Cast<TState>()
            .Where(e => !stateMapValidation.HasTopLevelState(e))
            .ToList();

        Errors.Add(new ValidationError<TState, TStimulus>
        {
            Reason = $"States {string.Join(",", states_not_in_state_map)} is not registered with the state map.",
            ErrorStates = states_not_in_state_map
        });

        return Result;
    }
}

public sealed class UnreachableStates<TState, TStimulus> : AbstractGraphRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        var traverse_results = TraverseStateMachine(initialState, stateMapValidation);
        if (traverse_results.NonReachableNodes.Any())
        {
            Errors.Add(new ValidationError<TState, TStimulus>
            {
                Reason = $"The following states are registered but not reachable: {string.Join(",", traverse_results.NonReachableNodes)}",
                ErrorStates = traverse_results.NonReachableNodes
            });
        }

        return Result;
    }
}

// TODO Track cycle path
public sealed class NoCycles<TState, TStimulus> : AbstractGraphRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        var traverse_results = TraverseStateMachine(initialState, stateMapValidation);
        if (traverse_results.IsCyclic)
        {
            Errors.Add(new ValidationError<TState, TStimulus> {Reason = $"There is a cycle in the state machine"});
        }

        return Result;
    }
}

public sealed class UnreachableAction<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        var unreachable_enter_actions = DoFindUnreachableActions(enterRegistryValidation, stateMapValidation);
        var unreachable_leave_actions = DoFindUnreachableActions(leaveRegistryValidation, stateMapValidation);

        if (unreachable_enter_actions.Any())
        {
            Errors.Add(new ValidationError<TState, TStimulus>
            {
                Reason = $"The following transitions can never trigger an transitionAction: {string.Join(",", unreachable_enter_actions)}",
                ErrorTransitions = unreachable_enter_actions
            });
        }

        if (unreachable_leave_actions.Any())
        {
            Errors.Add(new ValidationError<TState, TStimulus>
            {
                Reason = $"The following transitions can never trigger an transitionAction: {string.Join(",", unreachable_leave_actions)}",
                ErrorTransitions = unreachable_leave_actions
            });
        }

        return Result;
    }

    
    private static IReadOnlyList<ITransition<TState, TStimulus>> DoFindUnreachableActions(
        IActionRegistryValidation<TState, TStimulus> actionRegistryValidation,
        IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        var registered_transition_triggers = actionRegistryValidation.TransitionActionTransitions;
        
        return registered_transition_triggers.Where(transition => !stateMapValidation.IsTransitionRegistered(transition)).ToList();
    }
}

public sealed class UnreachableGuard<TState, TStimulus> : AbstractValidationRule<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public override IValidationResult<TState, TStimulus> Run(TState initialState, IStateMapValidation<TState, TStimulus> stateMapValidation,
        IActionRegistryValidation<TState, TStimulus> enterRegistryValidation, IActionRegistryValidation<TState, TStimulus> leaveRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        var unreachable_guards = DoFindUnreachableGuards(guardRegistryValidation, stateMapValidation);

        if (unreachable_guards.Any())
        {
            Errors.Add(new ValidationError<TState, TStimulus>
            {
                Reason = $"The following transitions will never trigger their registered transitionGuardRegistry: {string.Join(",", unreachable_guards)}",
                ErrorTransitions = unreachable_guards
            });
        }

        return Result;
    }

    private static IReadOnlyList<ITransition<TState, TStimulus>> DoFindUnreachableGuards(
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation,
        IStateMapValidation<TState, TStimulus> stateMapValidation)
    {
        var registered_transitions = guardRegistryValidation.GuardTransitions;
        return registered_transitions.Where(transition => !stateMapValidation.IsTransitionRegistered(transition)).ToList();
    }
}