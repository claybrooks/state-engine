using StateEngine;
using StateEngine.Deferred;

var engine = new Builder<State, Stimulus>(State.Start)
    // Global level actions
    // Will trigger whenever any state is entered/left.
    //.WithEnterAction(t => Console.WriteLine($"Global Enter: {t.From} -> {t.To} : {t.Reason}"))
    //.WithLeaveAction(t => Console.WriteLine($"Global Leave: {t.From} -> {t.To} : {t.Reason}"))

    .WithState(State.Start, startState =>
    {
        // State level actions
        // Will trigger whenever this state is entered or left
        startState.WithEnterAction(t => Console.WriteLine($"StartState Enter: {t.From} -> {t.To} : {t.Reason}"));
        startState.WithLeaveAction(t => Console.WriteLine($"StartState Leave: {t.From} -> {t.To} : {t.Reason}"));

        // Transition level actions
        // Will trigger whenever the specific transition occurs
        startState.WithEnterAction(
            State.Middle, // The state being left
            Stimulus.GoToStart, // The reason the transition occurred
            t => Console.WriteLine($"StartStateEnterAction: {t.From} -> {t.To} : {t.Reason}"));

        startState.WithLeaveAction(
            State.Middle, // The state being entered
            Stimulus.GoToMiddle, // The reason the transition occurred
            t => Console.WriteLine($"StartStateLeaveAction: {t.From} -> {t.To} : {t.Reason}"));

        startState.CanTransitionTo(State.Middle, Stimulus.GoToMiddle);
    })
    .WithState(State.Middle, middleState =>
    {
        middleState.WithEnterAction(t =>
        {
            Console.WriteLine($"MiddleState Enter: {t.From} -> {t.To} : {t.Reason}");
        });
        middleState.CanTransitionTo(State.End, Stimulus.GoToEnd);
        middleState.CanTransitionTo(State.Start, Stimulus.GoToStart);

        // This guard will protect transitioning in to this state
        middleState.WithEnterGuard(State.Start, Stimulus.GoToMiddle, t => true);
        // This guard will protect transitioning out of this state
        middleState.WithLeaveGuard(State.Start, Stimulus.GoToStart, t => true);
    })
    .WithState(State.End, endState =>
    {
        endState.WithEnterAction(t =>
        {
            Console.WriteLine($"EndState Enter: {t.From} -> {t.To} : {t.Reason}");
        });
        // State level actions
        endState.WithEnterAction(t => Console.WriteLine($"EndStateEnterAction: {t.From} -> {t.To} : {t.Reason}"));
        endState.WithLeaveAction(t => Console.WriteLine($"EndStateLeaveAction: {t.From} -> {t.To} : {t.Reason}"));
    })
    //.BuildDeferredStateEngine()
    .BuildDeferredStateEngine();

Console.WriteLine("GoToMiddle");
await engine.PostAsync(Stimulus.GoToMiddle).ConfigureAwait(false);
Console.WriteLine("GoToStart");
await engine.PostAsync(Stimulus.GoToStart).ConfigureAwait(false);
Console.WriteLine("GoToEnd");
await engine.PostAsync(Stimulus.GoToEnd).ConfigureAwait(false);
Console.WriteLine("GoToMiddle");
await engine.PostAsync(Stimulus.GoToMiddle).ConfigureAwait(false);
Console.WriteLine("GoToEnd");
await engine.PostAsync(Stimulus.GoToEnd).ConfigureAwait(false);

//await engine.AwaitIdleAsync();

engine.Dispose();
Console.WriteLine("End");
Console.ReadKey();

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