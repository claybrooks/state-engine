using StateEngine;
using StateEngine.Deferred;
using StateEngine.Visualizer;

var builder = new Builder<State, Stimulus>(State.Start)
    // Global level actions
    // Will trigger whenever any state is entered/left.
    //.WithEnterAction(t => Console.WriteLine($"Global Enter: {t.From} -> {t.To} : {t.Reason}"))
    //.WithLeaveAction(t => Console.WriteLine($"Global Leave: {t.From} -> {t.To} : {t.Reason}"))

    .WithState(State.Start, startState =>
    {
        // State level actions
        // Will trigger whenever this state is entered or left
        startState.WithEnterAction(t => Console.WriteLine($"StartState Global Enter: {t.From} -> {t.To} : {t.Reason}"));
        startState.WithLeaveAction(t => Console.WriteLine($"StartState Global Leave: {t.From} -> {t.To} : {t.Reason}"));

        // Transition level actions
        // Will trigger whenever the specific transition occurs
        startState.WithEnterAction(
            State.Middle, // The state being left
            Stimulus.GoToStart, // The reason the transition occurred
            t => Console.WriteLine($"MiddleState -> StartState Enter Action: {t.From} -> {t.To} : {t.Reason}"));

        startState.WithLeaveAction(
            State.Middle, // The state being entered
            Stimulus.GoToMiddle, // The reason the transition occurred
            t => Console.WriteLine($"StartState -> MiddleState Leave Action: {t.From} -> {t.To} : {t.Reason}"));

        startState.CanTransitionTo(State.Middle, Stimulus.GoToMiddle);
    })
    .WithState(State.Middle, middleState =>
    {
        middleState.WithEnterAction(t =>
        {
            Console.WriteLine($"MiddleState Global Enter: {t.From} -> {t.To} : {t.Reason}");
        });
        middleState.CanTransitionTo(State.End, Stimulus.GoToEnd);
        middleState.CanTransitionTo(State.Start, Stimulus.GoToStart);

        middleState.WithEnterGuard(t =>
        {
            Console.WriteLine("MiddleState Global Entry Guard");
            return true;
        });

        // This guard will protect transitioning in to this state
        middleState.WithEnterGuard(State.Start, Stimulus.GoToMiddle, t =>
        {
            Console.WriteLine("State -> MiddleState Entry Guard");
            return true;
        });
        // This guard will protect transitioning out of this state
        middleState.WithLeaveGuard(State.Start, Stimulus.GoToStart, t =>
        {
            Console.WriteLine("MiddleState -> State Leave Guard");
            return true;
        });
    })
    .WithState(State.End, endState =>
    {
        endState.WithEnterAction(t => Console.WriteLine($"EndState Global Enter: {t.From} -> {t.To} : {t.Reason}"));
        endState.WithLeaveAction(t => Console.WriteLine($"EndState Global Leave: {t.From} -> {t.To} : {t.Reason}"));
    });

var visualizer = builder.Visualizer<VisualizerFactory<State, Stimulus>>();
await visualizer.CreateDotAsync("test", "test.dot");

var engine = builder.BuildStateEngine();
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