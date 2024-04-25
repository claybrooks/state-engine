using System.Collections;

namespace StateEngine.Tests.Stubs;

public class StubHistory<TState, TStimulus> : IHistory<TState, TStimulus>
    where TState : struct where TStimulus : struct
{
    public IEnumerator<IHistoryItem<TState, TStimulus>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Enabled { get; set; }
    public bool IsUnbounded { get; }
    public void Add(TState from, TState to, TStimulus? when)
    {
        throw new NotImplementedException();
    }

    public void MakeBounded(int size)
    {
        throw new NotImplementedException();
    }

    public void MakeUnbounded()
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }
}