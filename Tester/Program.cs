using FluentState;
using FluentState.Machine;
using FluentState.MachineParts;
using Tester;

void PlayQuickStopAction(Transition<State, Stimulus> transition)
{
    Console.WriteLine($"Playing special animation for {transition.From}->{transition.To} because of {transition.Reason}");
}

var state_machine = new StateMachineBuilder<State, Stimulus>(State.Idle)
    .WithUnboundedHistory()
    .WithEnterAction(transition => { Console.WriteLine($"Entering {transition.To}"); })
    .WithEnterAction(new AnimationAction())
    .WithLeaveAction(transition => { Console.WriteLine($"Leaving {transition.From}"); })
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
            state_machine.Post(Stimulus.Walk);
            break;
        case ConsoleKey.C:
            state_machine.Post(Stimulus.Crouch);
            break;
        case ConsoleKey.R:
            state_machine.Post(Stimulus.Run);
            break;
        case ConsoleKey.S:
            state_machine.Post(Stimulus.Stop);
            break;
        case ConsoleKey.Q:
            state_machine.Post(Stimulus.QuickStop);
            break;
    }

} while (key != ConsoleKey.Escape);

//var serializer = new FluentState.Persistence.JsonSerializer<State, Stimulus>(new StateTypeConverter(), new StimulusTypeConverter());
//await serializer.Save(state_machine, "stateMachine.json");

state_machine.Post(Stimulus.Walk);
state_machine.Post(Stimulus.Crouch);
state_machine.Post(Stimulus.Stop);
state_machine.Post(Stimulus.Stop);
state_machine.Post(Stimulus.Run);
state_machine.Post(Stimulus.QuickStop);
state_machine.Post(Stimulus.Crouch);
state_machine.Post(Stimulus.Walk);
state_machine.Post(Stimulus.Stop);
state_machine.Post(Stimulus.Stop);

namespace Tester
{
    public class AnimationAction : IAction<State, Stimulus>
    {
        public void OnTransition(Transition<State, Stimulus> transition)
        {
            Console.WriteLine($"Playing {transition.From}->{transition.To} animation");
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
}