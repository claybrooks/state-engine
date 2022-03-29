using System;
using System.Collections.Generic;

namespace FluentState.History
{
    public class HistoryItem<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        public TState EnteringState;
        public TState LeavingState;
        public TStimulus Reason;
        public DateTime When;
    }

    public interface IStateMachineHistory<TState, TStimulus> : IEnumerable<HistoryItem<TState, TStimulus>>
        where TState : struct
        where TStimulus : struct
    {
        bool Enabled { get; set; }
        bool IsUnbounded { get; }
        void Add(TState enteringState, TState leavingState, TStimulus why);
        void MakeBounded(int size);
        void MakeUnbounded();
        void Clear();
    }
}
