using FluentState.Builder;
using Tester;

void PlayAnimation(State enteringState, State leavingState, Stimulus reason)
{
    Console.WriteLine($"Playing {leavingState}->{enteringState} animation");
}

void PlayQuickStopAction(State enteringState, State leavingState, Stimulus reason)
{
    Console.WriteLine($"Playing special animation for {leavingState}->{enteringState} because of {reason}");
}

var dateOfLastQuickStop = DateTime.UnixEpoch;
int quickStopCoolDownInSeconds = 5;
bool QuickStopCooldownCheck(State from, State to, Stimulus reason)
{
    var success = (DateTime.Now - dateOfLastQuickStop).TotalSeconds > quickStopCoolDownInSeconds;
    if (success)
    {
        dateOfLastQuickStop = DateTime.Now;
    }
    return success;
}

var stateMachine = new AsyncStateMachineBuilder<State, Stimulus>(State.Idle)
    .WithUnboundedHistory()
    .WithEnterAction((fromState, toState, reason) => { Console.WriteLine($"Entering {toState}"); })
    .WithEnterAction(PlayAnimation)
    .WithLeaveAction((fromState, toState, reason) => { Console.WriteLine($"Leaving {fromState}"); })
    .WithState(State.Idle)
        .CanTransitionTo(State.Walking, Stimulus.Walk)
        .CanTransitionTo(State.Running, Stimulus.Run)
        .CanTransitionTo(State.Crouched, Stimulus.Crouch)
        .Build()
    .WithState(State.Walking)
        .CanTransitionTo(State.Running, Stimulus.Run)
        .CanTransitionTo(State.Idle, Stimulus.Stop)
        .CanTransitionTo(State.CrouchWalking, Stimulus.Crouch)
        .Build()
    .WithState(State.Running)
        .CanTransitionTo(State.Walking, Stimulus.Walk)
        .CanTransitionTo(State.Idle, Stimulus.Stop)
        .CanTransitionTo(State.Idle, Stimulus.QuickStop, new List<Action<State, State, Stimulus>> () { PlayQuickStopAction }, new List<Func<State, State, Stimulus, bool>>() { QuickStopCooldownCheck })
        .Build()
    .WithState(State.Crouched)
        .CanTransitionTo(State.CrouchWalking, Stimulus.Walk)
        .CanTransitionTo(State.Idle, Stimulus.Stop)
        .Build()
    .WithState(State.CrouchWalking)
        .CanTransitionTo(State.Walking, Stimulus.Walk)
        .CanTransitionTo(State.Crouched, Stimulus.Stop)
        .Build()
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
            await stateMachine.Post(Stimulus.Walk);
            break;
        case ConsoleKey.C:
            await stateMachine.Post(Stimulus.Crouch);
            break;
        case ConsoleKey.R:
            await stateMachine.Post(Stimulus.Run);
            break;
        case ConsoleKey.S:
            await stateMachine.Post(Stimulus.Stop);
            break;
        case ConsoleKey.Q:
            await stateMachine.Post(Stimulus.QuickStop);
            break;
    }

} while (key != ConsoleKey.Escape);


var serializer = new FluentState.Persistence.JsonSerializer<State, Stimulus>(new StateTypeConverter());
await serializer.Save(stateMachine, "stateMachine.json");

await stateMachine.Post(Stimulus.Walk);
await stateMachine.Post(Stimulus.Crouch);
await stateMachine.Post(Stimulus.Stop);
await stateMachine.Post(Stimulus.Stop);
await stateMachine.Post(Stimulus.Run);
await stateMachine.Post(Stimulus.QuickStop);
await stateMachine.Post(Stimulus.Crouch);
await stateMachine.Post(Stimulus.Walk);
await stateMachine.Post(Stimulus.Stop);
await stateMachine.Post(Stimulus.Stop);

await stateMachine.AwaitIdleAsync();

await serializer.Load(stateMachine, "stateMachine.json");

stateMachine.Dispose();