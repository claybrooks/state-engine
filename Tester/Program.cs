using FluentState;
using FluentState.Machine;
using FluentState.MachineParts;
using Tester;

void PlayQuickStopAction(Transition<State, Stimulus> transition)
{
    Console.WriteLine($"Playing special animation for {transition.From}->{transition.To} because of {transition.Reason}");
}

var state_machine = new AsyncStateMachineBuilder<State, Stimulus>(State.Idle)
    .WithUnboundedHistory()
    .WithEnterAction<DebugTransition>()
    .WithEnterAction(new AnimationAction())
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

            .WithLeaveGuard<QuickStopGuard>(State.Idle, Stimulus.QuickStop)
            .WithLeaveAction(State.Idle, Stimulus.QuickStop, PlayQuickStopAction);
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
    .Build();

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
            await state_machine.PostAsync(Stimulus.Walk);
            break;
        case ConsoleKey.C:
            await state_machine.PostAsync(Stimulus.Crouch);
            break;
        case ConsoleKey.R:
            await state_machine.PostAsync(Stimulus.Run);
            break;
        case ConsoleKey.S:
            await state_machine.PostAsync(Stimulus.Stop);
            break;
        case ConsoleKey.Q:
            await state_machine.PostAsync(Stimulus.QuickStop);
            break;
    }

} while (key != ConsoleKey.Escape);

//var serializer = new FluentState.Persistence.JsonSerializer<State, Stimulus>(new StateTypeConverter(), new StimulusTypeConverter());
//await serializer.Save(state_machine, "stateMachine.json");

await state_machine.PostAsync(Stimulus.Walk);
await state_machine.PostAsync(Stimulus.Crouch);
await state_machine.PostAsync(Stimulus.Stop);
await state_machine.PostAsync(Stimulus.Stop);
await state_machine.PostAsync(Stimulus.Run);
await state_machine.PostAsync(Stimulus.QuickStop);
await state_machine.PostAsync(Stimulus.Crouch);
await state_machine.PostAsync(Stimulus.Walk);
await state_machine.PostAsync(Stimulus.Stop);
await state_machine.PostAsync(Stimulus.Stop);

await state_machine.DisposeAsync();

public class AnimationAction : IAction<State, Stimulus>
{
    public void OnTransition(Transition<State, Stimulus> transition)
    {
        Console.WriteLine($"Playing {transition.From}->{transition.To} animation");
    }
}

public class DebugTransition: IAction<State, Stimulus>
{
    public void OnTransition(Transition<State, Stimulus> transition)
    {
        Console.WriteLine($"Leaving {transition.From}, Entering {transition.To}, Because Of {transition.Reason}");
    }
}

public class QuickStopGuard : IGuard<State, Stimulus>
{
    private DateTime _dateOfLastQuickStop = DateTime.UnixEpoch;

    public bool Check(Transition<State, Stimulus> transition)
    { 
        var success = (DateTime.Now - _dateOfLastQuickStop).TotalSeconds > 5;
        if (success)
        {
            _dateOfLastQuickStop = DateTime.Now;
        }
        return success;
    }
}
