# StateEngine

StateEngine is a library for building state machines.

## Installation

Install via Nuget

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

# Examples

Refer to the following enums for the following examples
```csharp
public enum State
{
    Start,
    Middle,
    End
}

public enum Stimulus
{
    GoToStart,
    GoToMiddle,
    GoToEnd
}
```
## Builder

A fluent style api is provided to build your state-machine.

```csharp
using StateEngine;

var engine = new Builder<State, Stimulus>(State.Start)
    .WithState(State.Start, startState =>
    {
        startState.CanTransitionTo(State.Middle, Stimulus.GoToMiddle);
    })
    .WithState(State.Middle, middleState =>
    {
        middleState.CanTransitionTo(State.End, Stimulus.GoToEnd);
        middleState.CanTransitionTo(State.Start, Stimulus.GoToStart);
    })
    .WithState(State.End, endState =>
    {
        // Final state, don't allow any transition out of end
    })
    .Build<StateMachineFactory<State, Stimulus>>();
```

The frame of reference for registering transitions can be changed to suite your needs.  In the above example, ```.CanTransitionTo(...)``` was used.  You can use ```.CanTransitionFrom(...)``` if that feels more natural when registering state transitions.  In some cases, it may be easier to use ```.CanTransitionFrom(...)``` because the ```Stimulus``` used in the registration is logically related to the ```StateBuilder``` section you are in.

```csharp
var engine = new StateEngineBuilder<State, Stimulus>(State.Start)
    .WithState(State.Start, startState =>
    {
        startState.CanTransitionFrom(State.Middle, Stimulus.GoToStart);
    })
    .WithState(State.Middle, middleState =>
    {
        middleState.CanTransitionFrom(State.Start, Stimulus.GoToMiddle);
    })
    .WithState(State.End, endState =>
    {
        // Final state, don't allow any transition out of end
        endState.CanTransitionFrom(State.Middle, Stimulus.GoToEnd);
    })
    .Build<StateMachineFactory<State, Stimulus>>();
```

You can specify actions at varying levels within the state engine
```csharp
using StateEngine;

var engine = new StateEngineBuilder<State, Stimulus>(State.Start)
    // Global level actions
    // Will trigger whenever any state is entered/left.
    .WithEnterAction(t => Console.WriteLine($"{t.From} -> {t.To} : {t.Reason}"))
    .WithLeaveAction(t => Console.WriteLine($"{t.From} -> {t.To} : {t.Reason}"))
    .WithState(State.Start, startState =>
    {
        // State level actions
        // Will trigger whenever this state is entered or left
        startState.WithEnterAction(t => Console.WriteLine($"{t.From} -> {t.To} : {t.Reason}"));
        startState.WithLeaveAction(t => Console.WriteLine($"{t.From} -> {t.To} : {t.Reason}"));

        // Transition level actions
        // Will trigger whenever the specific transition occurs
        startState.WithEnterAction(
            State.Middle, // The state being left
            Stimulus.GoToStart, // The reason the transition occurred
            t => Console.WriteLine($"{t.From} -> {t.To} : {t.Reason}"));

        startState.WithLeaveAction(
            State.Middle, // The state being entered
            Stimulus.GoToMiddle, // The reason the transition occurred
            t => Console.WriteLine($"{t.From} -> {t.To} : {t.Reason}"));

        startState.CanTransitionTo(State.Middle, Stimulus.GoToMiddle);
    })
    .WithState(State.Middle, middleState =>
    {
        middleState.CanTransitionTo(State.End, Stimulus.GoToEnd);
        middleState.CanTransitionTo(State.Start, Stimulus.GoToStart);
    })
    .WithState(State.End, endState =>
    {
        // Final state, don't allow any transition out of end
    })
    .Build<StateMachineFactory<State, Stimulus>>();
```
Actions registered to ```Leave``` events are triggered prior to the state transition.  Actions registered to ```Enter``` events are triggered immediately after the state transition.

Actions are run in the following order:
 1. ```Global Level``` ```Leave``` events, in the order they were registered
 2. ```State Level``` ```Leave``` events, in the order they were registered
 3. ```Transition Level``` ```Leave``` eventse, in the order they were registered.
 4. ```Global Level``` ```Enter``` events, in the order they were registered
 5. ```State Level``` ```Enter``` events, in the order they were registered
 6. ```Transition Level``` ```Enter``` events, in the order they were registered

You can register guards to disallow transitions within the engine.  All guards are registered at the transition level.  All guards are run in the order they are registered.
```csharp
.WithState(State.Middle, middleState =>
    {
        middleState.CanTransitionTo(State.End, Stimulus.GoToEnd);
        middleState.CanTransitionTo(State.Start, Stimulus.GoToStart);

        // This guard will always allow transitioning in to this state
        middleState.WithEnterGuard(State.Start, Stimulus.GoToMiddle, t => { return true; });

        // This guard will always block transitioning out of this state to the State.Start state
        middleState.WithLeaveGuard(State.Start, Stimulus.GoToStart, t => { return false; });
    })
```