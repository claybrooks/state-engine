using FluentAssertions;
using StateEngine.Tests.Stubs;

namespace StateEngine.Tests;

public enum State
{
    State1,
    State2
}

public enum Stimulus
{
    TransitionToState1,
    TransitionToState2
}

public class StateBuilderTests
{
    private readonly StubTransitionGuardRegistry<State, Stimulus> _transitionGuardRegistry;
    private readonly StubStateMap<State, Stimulus> _stateMap;
    private readonly StubTransitionActionRegistry<State, Stimulus> _enterActionRegistry;
    private readonly StubTransitionActionRegistry<State, Stimulus> _leaveActionRegistry;

    private readonly StateBuilder<State, Stimulus> _sut;

    public StateBuilderTests()
    {
        _transitionGuardRegistry = new StubTransitionGuardRegistry<State, Stimulus>();
        _stateMap = new StubStateMap<State, Stimulus>();
        _enterActionRegistry = new StubTransitionActionRegistry<State, Stimulus>();
        _leaveActionRegistry = new StubTransitionActionRegistry<State, Stimulus>();

        _sut = new StateBuilder<State, Stimulus>(State.State1, _transitionGuardRegistry, _stateMap,
            _enterActionRegistry, _leaveActionRegistry);
    }

    [Fact]
    public void InitialStateIsEmpty()
    {
        _stateMap.AnythingRegistered.Should().BeFalse("StateBuilder wasn't invoked");
        _transitionGuardRegistry.AnythingRegistered.Should().BeFalse("Guard registrations were not called yet");
        _enterActionRegistry.AnythingRegistered.Should().BeFalse("Enter action registrations were not called yet");
        _leaveActionRegistry.AnythingRegistered.Should().BeFalse("Leave action registrations were not called yet");
    }

    [Fact]
    public void StateTransitionRegistration()
    {
        _sut.CanTransitionTo(State.State2, Stimulus.TransitionToState2);

        _stateMap.AnythingRegistered.Should().BeTrue("CanTransitionTo was invoked");
        _transitionGuardRegistry.AnythingRegistered.Should().BeFalse("Guard registrations were not called yet");
        _enterActionRegistry.AnythingRegistered.Should().BeFalse("Enter action registrations were not called yet");
        _leaveActionRegistry.AnythingRegistered.Should().BeFalse("Leave action registrations were not called yet");
    }

    [Fact]
    public void DelegateEnterGuardTransitionRegistration()
    {
        _sut.WithEnterGuard(State.State2, Stimulus.TransitionToState1, transition => false);

        _transitionGuardRegistry.AnythingRegistered.Should().BeTrue("a guard was registered");
        _transitionGuardRegistry.LastRegisteredTransition
            .Should().NotBeNull("a guard was registered").And
            .BeEquivalentTo(new Transition<State, Stimulus>()
                {From = State.State2, To = State.State1, Reason = Stimulus.TransitionToState1});
    }

    [Fact]
    public void GenericsEnterGuardTransitionRegistration()
    {
        _sut.WithEnterGuard<StubTransitionGuard<State, Stimulus>>(State.State2, Stimulus.TransitionToState1);

        _transitionGuardRegistry.AnythingRegistered.Should().BeTrue("a guard was registered");
        _transitionGuardRegistry.LastRegisteredTransition
            .Should().NotBeNull("a guard was registered").And
            .BeEquivalentTo(new Transition<State, Stimulus>()
                {From = State.State2, To = State.State1, Reason = Stimulus.TransitionToState1});
    }

    [Fact]
    public void InstantiatedEnterGuardTransitionRegistration()
    {
        _sut.WithEnterGuard(State.State2, Stimulus.TransitionToState1, new StubTransitionGuard<State, Stimulus>());

        _transitionGuardRegistry.AnythingRegistered.Should().BeTrue("a guard was registered");
        _transitionGuardRegistry.LastRegisteredTransition
            .Should().NotBeNull("a guard was registered").And
            .BeEquivalentTo(new Transition<State, Stimulus>()
                {From = State.State2, To = State.State1, Reason = Stimulus.TransitionToState1});
    }

    [Fact]
    public void DelegateLeaveGuardTransitionRegistration()
    {
        _sut.WithLeaveGuard(State.State2, Stimulus.TransitionToState2, transition => false);

        _transitionGuardRegistry.AnythingRegistered.Should().BeTrue("a guard was registered");
        _transitionGuardRegistry.LastRegisteredTransition
            .Should().NotBeNull("a guard was registered").And
            .BeEquivalentTo(new Transition<State, Stimulus>()
                {From = State.State1, To = State.State2, Reason = Stimulus.TransitionToState2});
    }

    [Fact]
    public void GenericsLeaveGuardTransitionRegistration()
    {
        _sut.WithLeaveGuard<StubTransitionGuard<State, Stimulus>>(State.State2, Stimulus.TransitionToState2);

        _transitionGuardRegistry.AnythingRegistered.Should().BeTrue("a guard was registered");
        _transitionGuardRegistry.LastRegisteredTransition
            .Should().NotBeNull("a guard was registered").And
            .BeEquivalentTo(new Transition<State, Stimulus>()
                {From = State.State1, To = State.State2, Reason = Stimulus.TransitionToState2});
    }

    [Fact]
    public void InstantiatedLeaveGuardTransitionRegistration()
    {
        _sut.WithLeaveGuard(State.State2, Stimulus.TransitionToState2, new StubTransitionGuard<State, Stimulus>());

        _transitionGuardRegistry.AnythingRegistered.Should().BeTrue("a guard was registered");
        _transitionGuardRegistry.LastRegisteredTransition
            .Should().NotBeNull("a guard was registered").And
            .BeEquivalentTo(new Transition<State, Stimulus>()
                {From = State.State1, To = State.State2, Reason = Stimulus.TransitionToState2});
    }

    [Fact]
    public void DelegateStateEnterActionRegistration()
    {
        _sut.WithEnterAction(t => { });
        _enterActionRegistry.AnythingRegistered.Should().BeTrue("state action was registered");
        _enterActionRegistry.StateActionRegistered.Should().BeTrue("state level action was registered");
        _enterActionRegistry.GlobalActionRegistered.Should().BeFalse("global action was not registered");
        _enterActionRegistry.TransitionActionRegistered.Should().BeFalse("transition level action was not registered");
    }

    [Fact]
    public void GenericsStateEnterActionRegistration()
    {
        _sut.WithEnterAction<StubTransitionAction<State, Stimulus>>();
        _enterActionRegistry.AnythingRegistered.Should().BeTrue("state action was registered");
        _enterActionRegistry.StateActionRegistered.Should().BeTrue("state level action was registered");
        _enterActionRegistry.GlobalActionRegistered.Should().BeFalse("global action was not registered");
        _enterActionRegistry.TransitionActionRegistered.Should().BeFalse("transition level action was not registered");
    }

    [Fact]
    public void InstantiatedStateEnterActionRegistration()
    {
        _sut.WithEnterAction(new StubTransitionAction<State, Stimulus>());
        _enterActionRegistry.AnythingRegistered.Should().BeTrue("state action was registered");
        _enterActionRegistry.StateActionRegistered.Should().BeTrue("state level action was registered");
        _enterActionRegistry.GlobalActionRegistered.Should().BeFalse("global action was not registered");
        _enterActionRegistry.TransitionActionRegistered.Should().BeFalse("transition level action was not registered");
    }
    

    [Fact]
    public void DelegateStateLeaveActionRegistration()
    {
        _sut.WithLeaveAction(t => { });
        _leaveActionRegistry.AnythingRegistered.Should().BeTrue("state action was registered");
        _leaveActionRegistry.StateActionRegistered.Should().BeTrue("state level action was registered");
        _leaveActionRegistry.GlobalActionRegistered.Should().BeFalse("global action was not registered");
        _leaveActionRegistry.TransitionActionRegistered.Should().BeFalse("transition level action was not registered");
    }

    [Fact]
    public void GenericsStateLeaveActionRegistration()
    {
        _sut.WithLeaveAction<StubTransitionAction<State, Stimulus>>();
        _leaveActionRegistry.AnythingRegistered.Should().BeTrue("state action was registered");
        _leaveActionRegistry.StateActionRegistered.Should().BeTrue("state level action was registered");
        _leaveActionRegistry.GlobalActionRegistered.Should().BeFalse("global action was not registered");
        _leaveActionRegistry.TransitionActionRegistered.Should().BeFalse("transition level action was not registered");
    }

    [Fact]
    public void InstantiatedStateLeaveActionRegistration()
    {
        _sut.WithLeaveAction(new StubTransitionAction<State, Stimulus>());
        _leaveActionRegistry.AnythingRegistered.Should().BeTrue("state action was registered");
        _leaveActionRegistry.StateActionRegistered.Should().BeTrue("state level action was registered");
        _leaveActionRegistry.GlobalActionRegistered.Should().BeFalse("global action was not registered");
        _leaveActionRegistry.TransitionActionRegistered.Should().BeFalse("transition level action was not registered");
    }
}
