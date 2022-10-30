using System;
using System.Collections;
using System.Collections.Generic;

namespace FluentState.History;

public class HistoryItem<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public TState To;
    public TState From;
    public TStimulus Reason;
    public DateTimeOffset When;
}

public interface IStateMachineHistory<TState, TStimulus> : IEnumerable<HistoryItem<TState, TStimulus>>
    where TState : struct
    where TStimulus : struct
{
    bool Enabled { get; set; }
    bool IsUnbounded { get; }
    void Add(TState from, TState to, TStimulus when);
    void MakeBounded(int size);
    void MakeUnbounded();
    void Clear();
}

public class StateMachineHistory<TState, TStimulus> : IStateMachineHistory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly Queue<HistoryItem<TState, TStimulus>> _history = new();
    private int _size;

    public StateMachineHistory(int size = -1)
    {
        _size = size;
        Enabled = false;
    }

    public bool Enabled { get; set; }

    public bool IsUnbounded => _size < 0;

    public void MakeUnbounded()
    {
        _size = -1;
    }

    public void Clear()
    {
        _history.Clear();
    }

    public void Add(TState from, TState to, TStimulus when)
    {
        if (!Enabled)
        {
            return;
        }

        DoTrimToSize();
        _history.Enqueue(new HistoryItem<TState, TStimulus>{From = from, To = to, Reason = when, When = DateTimeOffset.Now});
    }

    public void MakeBounded(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than 0");
        }

        _size = size;
        DoTrimToSize();
    }

    public IEnumerator<HistoryItem<TState, TStimulus>> GetEnumerator()
    {
        return _history.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void DoTrimToSize()
    {
        if (IsUnbounded)
        {
            return;
        }

        while (_history.Count > _size)
        {
            _history.Dequeue();
        }
    }
}