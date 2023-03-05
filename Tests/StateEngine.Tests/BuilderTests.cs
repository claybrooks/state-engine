using FluentAssertions;
using StateEngine.Tests.Stubs;

namespace StateEngine.Tests;

public class BuilderTests
{
    private readonly Builder<State, Stimulus, StubStateMap<State, Stimulus>,
        StubTransitionActionRegistry<State, Stimulus>, StubTransitionGuardRegistry<State, Stimulus>,
        StubHistory<State, Stimulus>> _sut;

    public BuilderTests()
    {
        _sut = new Builder<State, Stimulus, StubStateMap<State, Stimulus>, StubTransitionActionRegistry<State, Stimulus>, StubTransitionGuardRegistry<State, Stimulus>, StubHistory<State, Stimulus>>(State.State1);
    }

    [Fact]
    public void InitialState()
    {
        var engine = _sut.Build<StubEngineFactory<State, Stimulus>>();
        engine.CurrentState.Should().Be(State.State1, "It is the initial param passed to the Builder constructor");
    }
}