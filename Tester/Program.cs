using FluentState.Builder;
using FluentState.Config;
using FluentState.Machine;
using Tester;

//void PlayAnimation(State enteringState, State leavingState, Stimulus reason)
//{
//    Console.WriteLine($"Playing {leavingState}->{enteringState} animation");
//}

void PlayQuickStopAction(State enteringState, State leavingState, Stimulus reason)
{
    Console.WriteLine($"Playing special animation for {leavingState}->{enteringState} because of {reason}");
}

void PrintEnteringState(State enteringState, State leavingState, Stimulus reason)
{
    Console.WriteLine($"Entering {enteringState}");
}

void PrintLeavingState(State enteringState, State leavingState, Stimulus reason)
{
    Console.WriteLine($"Leaving {leavingState}");
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

void ProcessMachine(IStateMachine<State, Stimulus> machine)
{
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
                machine.Post(Stimulus.Walk);
                break;
            case ConsoleKey.C:
                machine.Post(Stimulus.Crouch);
                break;
            case ConsoleKey.R:
                machine.Post(Stimulus.Run);
                break;
            case ConsoleKey.S:
                machine.Post(Stimulus.Stop);
                break;
            case ConsoleKey.Q:
                machine.Post(Stimulus.QuickStop);
                break;
        }

    } while (key != ConsoleKey.Escape);
}


//var stateMachine = new AsyncStateMachineBuilder<State, Stimulus>(State.Idle)
//    .WithUnboundedHistory()
//    .WithEnterAction((enteringState, leavingState, reason) => { Console.WriteLine($"Entering {enteringState}"); })
//    .WithEnterAction(PlayAnimation)
//    .WithLeaveAction((enteringState, leavingState, reason) => { Console.WriteLine($"Leaving {leavingState}"); })
//    .WithState(State.Idle)
//        .WithTransitionTo(State.Walking, Stimulus.Walk)
//        .WithTransitionTo(State.Running, Stimulus.Run)
//        .WithTransitionTo(State.Crouched, Stimulus.Crouch)
//        .Build()
//    .WithState(State.Walking)
//        .WithTransitionTo(State.Running, Stimulus.Run)
//        .WithTransitionTo(State.Idle, Stimulus.Stop)
//        .WithTransitionTo(State.CrouchWalking, Stimulus.Crouch)
//        .Build()
//    .WithState(State.Running)
//        .WithTransitionTo(State.Walking, Stimulus.Walk)
//        .WithTransitionTo(State.Idle, Stimulus.Stop)
//        .WithTransitionTo(
//            State.Idle,
//            Stimulus.QuickStop,
//            new List<Func<State, State, Stimulus, bool>>() { QuickStopCooldownCheck },
//            new List<Action<State, State, Stimulus>> () { PlayQuickStopAction },
//            null)
//        .Build()
//    .WithState(State.Crouched)
//        .WithTransitionTo(State.CrouchWalking, Stimulus.Walk)
//        .WithTransitionTo(State.Idle, Stimulus.Stop)
//        .WithTransitionTo(State.Running, Stimulus.Run)
//        .Build()
//    .WithState(State.CrouchWalking)
//        .WithTransitionTo(State.Running, Stimulus.Run)
//        .WithTransitionTo(State.Walking, Stimulus.Walk)
//        .WithTransitionTo(State.Crouched, Stimulus.Stop)
//        .Build()
//    .Build();


//var serializer = new FluentState.Persistence.DefaultJsonSerializer<State, Stimulus>(new StateTypeConverter(), new StimulusTypeConverter());
//await serializer.Save(stateMachine, "stateMachine.json");

//await stateMachine.Post(Stimulus.Walk);
//await stateMachine.Post(Stimulus.Crouch);
//await stateMachine.Post(Stimulus.Stop);
//await stateMachine.Post(Stimulus.Stop);
//await stateMachine.Post(Stimulus.Run);
//await stateMachine.Post(Stimulus.QuickStop);
//await stateMachine.Post(Stimulus.Crouch);
//await stateMachine.Post(Stimulus.Walk);
//await stateMachine.Post(Stimulus.Stop);
//await stateMachine.Post(Stimulus.Stop);

//await stateMachine.AwaitIdleAsync();

//await serializer.Load(stateMachine, "stateMachine.json");

//stateMachine.Dispose();

var actionProvider = new ActionProvider();
var guardProvider = new GuardProvider();

actionProvider.Actions.Add(nameof(PrintEnteringState), PrintEnteringState);
actionProvider.Actions.Add(nameof(PrintLeavingState), PrintLeavingState);
actionProvider.Actions.Add(nameof(PlayQuickStopAction), PlayQuickStopAction);
guardProvider.Guards.Add(nameof(QuickStopCooldownCheck), QuickStopCooldownCheck);

var stateMachineJson = "C:/Users/clay_/programming/bug-free-broccoli/Tester/statemachine.json";
var configStateMachine = new StateMachineBuilder<State, Stimulus>(State.Invalid)
    .WithConfig(new JsonConfigLoader<State, Stimulus>(stateMachineJson, new StateTypeConverter(), new StimulusTypeConverter(), actionProvider, guardProvider))
    .Build();

ProcessMachine(configStateMachine);