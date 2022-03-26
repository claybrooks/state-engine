using FluentState.Builder;
using Tester;

void PlayAnimation(State to, State from, Stimulus reason)
{
    Console.WriteLine($"Playing {from}->{to} animation");
}

void PlayOneOffAction(State to, State from, Stimulus reason)
{
    Console.WriteLine($"Playing special animation for {from}->{to} because of {reason}");
}

var stateMachine = new StateMachineBuilder<State, Stimulus>(State.Idle)
    .WithEnterAction((toState, fromState, reason) => { Console.WriteLine($"Entering {toState}"); })
    .WithEnterAction(PlayAnimation)
    .WithLeaveAction((toState, fromState, reason) => { Console.WriteLine($"Leaving {fromState}"); })
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
        .CanTransitionTo(State.Idle, Stimulus.QuickStop, PlayOneOffAction)
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
            stateMachine.Poke(Stimulus.Walk);
            break;
        case ConsoleKey.C:
            stateMachine.Poke(Stimulus.Crouch);
            break;
        case ConsoleKey.R:
            stateMachine.Poke(Stimulus.Run);
            break;
        case ConsoleKey.S:
            stateMachine.Poke(Stimulus.Stop);
            break;
        case ConsoleKey.Q:
            stateMachine.Poke(Stimulus.QuickStop);
            break;
    }

} while (key != ConsoleKey.Escape);
