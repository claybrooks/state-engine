using System.Collections;

namespace StateEngine;

public interface IHistoryItem<out TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public TState To { get; }
    public TState From { get; }
    public TStimulus? Reason { get; }
    public DateTimeOffset When { get; }
}

public interface IHistory<TState, TStimulus> : IEnumerable<IHistoryItem<TState, TStimulus>>
    where TState : struct
    where TStimulus : struct
{
    bool Enabled { get; set; }
    bool IsUnbounded { get; }
    void Add(TState from, TState to, TStimulus? when);
    void MakeBounded(int size);
    void MakeUnbounded();
    void Clear();
}

internal sealed class HistoryItem<TState, TStimulus> : IHistoryItem<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public TState To { get; set; }
    public TState From { get; set; }
    public TStimulus? Reason { get; set; }
    public DateTimeOffset When { get; set; }
}

public sealed class History<TState, TStimulus> : IHistory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    private readonly Queue<HistoryItem<TState, TStimulus>> _history = new();
    private int _size;
    
    public History() : this(-1)
    {
    }

    public History(int size)
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

    public void Add(TState from, TState to, TStimulus? when)
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

    public IEnumerator<IHistoryItem<TState, TStimulus>> GetEnumerator()
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
