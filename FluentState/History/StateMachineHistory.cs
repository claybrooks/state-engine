using System;
using System.Collections;
using System.Collections.Generic;

namespace FluentState.History
{
    public class StateMachineHistory<TState, TStimulus> : IStateMachineHistory<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        private readonly Queue<HistoryItem<TState, TStimulus>> _history = new Queue<HistoryItem<TState, TStimulus>>();
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

        public void Add(TState enteringState, TState leavingState, TStimulus why)
        {
            if (!Enabled)
            {
                return;
            }

            TrimToSize();

            _history.Enqueue(new HistoryItem<TState, TStimulus> { 
                EnteringState=enteringState,
                LeavingState=leavingState,
                Reason=why,
                When=DateTime.Now,
            });
        }

        public void MakeBounded(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than 0");
            }

            _size = size;
            TrimToSize();
        }

        public IEnumerator<HistoryItem<TState, TStimulus>> GetEnumerator()
        {
            return _history.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Private

        void TrimToSize()
        {
            // Unbounded
            if (IsUnbounded)
            {
                return;
            }

            while (_history.Count > _size)
            {
                _history.Dequeue();
            }
        }

        #endregion
    }
}
