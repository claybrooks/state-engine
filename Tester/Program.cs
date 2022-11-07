using StateEngine;
using StateEngine.Deferred;
using StateEngine.Validation;
using StateEngine.Visualizer;
using Tester;

void PlayQuickStopAction(ITransition<State, Stimulus> transition)
{
    Console.WriteLine($"Playing special animation for {transition.From}->{transition.To} because of {transition.Reason}");
}

var state_machine_builder = new PlayerAnimationStateEngineBuilder(State.Idle)
    .WithUnboundedHistory()
    .WithEnterAction(new AnimationTransitionAction())
    .WithState(State.Idle, sb =>
    {
        sb.CanTransitionTo(State.Walking, Stimulus.Walk)
            .CanTransitionTo(State.Running, Stimulus.Run)
            .CanTransitionTo(State.Crouched, Stimulus.Crouch);
    })
    .WithState(State.Walking, sb =>
    {
        sb.CanTransitionTo(State.Running, Stimulus.Run)
            .CanTransitionTo(State.Idle, Stimulus.Stop)
            .CanTransitionTo(State.CrouchWalking, Stimulus.Crouch);
    })
    .WithState(State.Running, sb =>
    {
        sb.CanTransitionTo(State.Walking, Stimulus.Walk)
            .CanTransitionTo(State.Idle, Stimulus.Stop)
            .CanTransitionTo(State.Idle, Stimulus.QuickStop)

            .WithLeaveGuard<QuickStopTransitionGuard>(State.Idle, Stimulus.QuickStop)
            .WithLeaveAction(State.Idle, Stimulus.QuickStop, PlayQuickStopAction, nameof(PlayQuickStopAction));
    })
    .WithState(State.Crouched, sb =>
    {
        sb.CanTransitionTo(State.CrouchWalking, Stimulus.Walk)
            .CanTransitionTo(State.Idle, Stimulus.Stop);
    })
    .WithState(State.CrouchWalking, sb =>
    {
        sb.CanTransitionTo(State.Walking, Stimulus.Walk)
            .CanTransitionTo(State.Crouched, Stimulus.Stop);
    })
    .WithState(State.Test, sb =>
    {
        sb.CanTransitionTo(State.Idle, Stimulus.QuickStop)
            .WithLeaveGuard<QuickStopTransitionGuard>(State.Idle, Stimulus.QuickStop)
            .WithLeaveAction(State.Idle, Stimulus.QuickStop, PlayQuickStopAction, nameof(PlayQuickStopAction));
    });

var validator = state_machine_builder.Validator<ValidatorFactory<State, Stimulus>>(Rules.Get<State, Stimulus>());
var results = validator.Validate();
if (results.Errors.Any())
{
    foreach (var error in results.Errors)
    {
        Console.WriteLine(error.Reason);
    }
}

var visualizer = state_machine_builder.Visualizer<VisualizerFactory<State, Stimulus>>();
visualizer.CreateDot("PlayerAnimation", "PlayerAnimation.dot");
var state_machine = state_machine_builder.Build();

ConsoleKey key;
do
{
    while (!Console.KeyAvailable)
    {
    }

    key = Console.ReadKey(true).Key;

    switch (key)
    {
        case ConsoleKey.W:
            await state_machine.Post(Stimulus.Walk);
            break;
        case ConsoleKey.C:
            await state_machine.Post(Stimulus.Crouch);
            break;
        case ConsoleKey.R:
            await state_machine.Post(Stimulus.Run);
            break;
        case ConsoleKey.S:
            await state_machine.Post(Stimulus.Stop);
            break;
        case ConsoleKey.Q:
            await state_machine.Post(Stimulus.QuickStop);
            break;
    }

} while (key != ConsoleKey.Escape);

//var serializer = new FluentState.Persistence.JsonSerializer<State, Stimulus>(new StateTypeConverter(), new StimulusTypeConverter());
//await serializer.Save(state_machine, "stateEngine.json");

await state_machine.Post(Stimulus.Walk);
await state_machine.Post(Stimulus.Crouch);
await state_machine.Post(Stimulus.Stop);
await state_machine.Post(Stimulus.Stop);
await state_machine.Post(Stimulus.Run);
await state_machine.Post(Stimulus.QuickStop);
await state_machine.Post(Stimulus.Crouch);
await state_machine.Post(Stimulus.Walk);
await state_machine.Post(Stimulus.Stop);
await state_machine.Post(Stimulus.Stop);

state_machine.Dispose();

Console.WriteLine("Exiting");

public class AnimationTransitionAction : ITransitionAction<State, Stimulus>
{
    public string Id => nameof(AnimationTransitionAction);

    public Task OnTransition(ITransition<State, Stimulus> transition)
    {
        Console.WriteLine($"Playing {transition.From}->{transition.To} animation");
        return Task.CompletedTask;
    }
}

public class DebugTransition: ITransitionAction<State, Stimulus>
{
    public string Id => nameof(DebugTransition);

    public Task OnTransition(ITransition<State, Stimulus> transition)
    {
        Console.WriteLine($"Leaving {transition.From}, Entering {transition.To}, Because Of {transition.Reason}");
        return Task.CompletedTask;
    }
}

public class QuickStopTransitionGuard : ITransitionGuard<State, Stimulus>
{
    private DateTime _dateOfLastQuickStop = DateTime.MinValue;

    public Task<bool> Check(ITransition<State, Stimulus> transition)
    { 
        var success = (DateTime.Now - _dateOfLastQuickStop).TotalSeconds > 5;
        if (success)
        {
            _dateOfLastQuickStop = DateTime.Now;
        }
        return Task.FromResult(success);
    }
}


public class PlayerAnimationStateEngineBuilder : DeferredStateEngineBuilder<State, Stimulus>
{
    public PlayerAnimationStateEngineBuilder(State initialState) : base(initialState)
    {
    }
}