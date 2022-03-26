**FluentState**

#

Getting Started
```csharp
/**
 * Uses the default StateMachine builder to easily build a state machine.
 */

enum State
{
    STATE_1,
    STATE_2,
    STATE_3,
}

enum Stimulus
{
    STIM_1,
    STIM_2,
    STIM_3,
}

var stateMachine = new StateMachineBuilder<State, Stimulus>(State.STATE_1)
    // Global enter action across all states
    .WithEnterAction((toState, fromState, reason) => { /* Custom Action Here */ })
    // Global leave action across all states
    .WithLeaveAction((toState, fromState, reason) => { /* Custom Action Here */ })
    // Start building a specific state
    .WithState(State.STATE_1)
        // This action is applied any time STATE_1 is entered, regardless of the previous state and stimulus
        .WithEnterAction((toState, fromState, reason) => { /* Custom Action Here */})
        // Define the transitions
        .CanTransitionTo(State.STATE_2, Stimulus.STIM_2)
        // This action will only trigger when going to STATE_3 from STATE_1 because of STIM_3
        .CanTransitionTo(State.STATE_3, Stimulus.STIM_3, (toState, fromState, reason) => { /* Custom Action Here */})
        .Build()

// Start working the state machine
stateMachine.Post(STIM_2);
)
```
#

AsyncStateMachine
```csharp
/**
 * Uses the default AsyncStateMachine builder to build an async state machine.
 * The usage is identical to the StateMachine builder
 */

enum State
{
    STATE_1,
    STATE_2,
    STATE_3,
}

enum Stimulus
{
    STIM_1,
    STIM_2,
    STIM_3,
}

var asyncStateMachine = new AsyncStateMachineBuilder<State, Stimulus>(State.STATE_1)
    // Global enter action across all states
    .WithEnterAction((toState, fromState, reason) => { /* Custom Action Here */ })
    // Global leave action across all states
    .WithLeaveAction((toState, fromState, reason) => { /* Custom Action Here */ })
    // Start building a specific state
    .WithState(State.STATE_1)
        // This action is applied any time STATE_1 is entered, regardless of the previous state and stimulus
        .WithEnterAction((toState, fromState, reason) => { /* Custom Action Here */})
        // Define the transitions
        .CanTransitionTo(State.STATE_2, Stimulus.STIM_2)
        // This action will only trigger when going to STATE_3 from STATE_1 because of STIM_3
        .CanTransitionTo(State.STATE_3, Stimulus.STIM_3, (toState, fromState, reason) => { /* Custom Action Here */})
        .Build()

// Start working the state machine
await asyncStateMachine.Post(STIM_2);

// STIM_2 will be processed in due time, continue with other logic
/* Other logic */

// Now wait for the state machine to be idle, which implies STIM_2 was processed
await asyncStateMachine.AwaitIdleAsync();

// Dispose to clean up the thread
// If it's important for all stimuli in the queue to be processed prior to cleanup, you must
// call AwaitIdleAsync(), else everything in the queue will be ignored on Dispose()
asyncStateMachine.Dispose();
```