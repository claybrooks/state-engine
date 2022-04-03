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
        /// <summary>
        /// Enable/Disable history collection.
        /// </summary>
        /// <remarks>
        /// Enabling and disabling of history will not affect the size or any history captured prior to this call.
        /// </remarks>
        bool Enabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool IsUnbounded { get; }

        /// <summary>
        /// Add's an item to the history buffer
        /// </summary>
        /// <param name="enteringState"></param>
        /// <param name="leavingState"></param>
        /// <param name="why"></param>
        void Add(TState enteringState, TState leavingState, TStimulus why);

        /// <summary>
        /// Add's an item to the history buffer
        /// </summary>
        /// <remarks>
        /// Callers should prefer <see cref="Add(TState, TState, TStimulus)"/>.  This is added here to support <see cref="FluentState.Persistence.ISerializer{TState, TStimulus}.Load(IStateMachine{TState, TStimulus}, string, System.Threading.CancellationToken)"/>
        /// </remarks>
        /// <param name="historyItem"></param>
        void Add(HistoryItem<TState, TStimulus> historyItem);

        /// <summary>
        /// Sets the new bounded size to <paramref name="size"/>
        /// </summary>
        /// <remarks>
        /// It is up to the implementation to decide how a unbounded -> bounded transition is handled.
        /// </remarks>
        /// <param name="size"></param>
        void MakeBounded(int size);

        /// <summary>
        /// Ensures the buffer is marked as unbounded.
        /// </summary>
        /// <remarks>
        /// Implementations should make an effort to not lose any history when a bounded -> unbounded transition occurs.
        /// </remarks>
        void MakeUnbounded();

        /// <summary>
        /// Clears the entire history buffer.
        /// </summary>
        void Clear();
    }
}
