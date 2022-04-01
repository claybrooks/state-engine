using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FluentState
{
    public class AsyncStateMachine<TState, TStimulus> : StateMachine<TState, TStimulus>, IAsyncStateMachine<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        private readonly Channel<TStimulus> _stimulusChannel = Channel.CreateUnbounded<TStimulus>();
        private readonly Thread _stimulusProcessingThread;
        private readonly CancellationTokenSource _internalThreadCancellationTokenSource = new CancellationTokenSource();

        public AsyncStateMachine(TState initialState) : base(initialState)
        {
            _stimulusProcessingThread = new Thread(() => { ProcessStimuli(_internalThreadCancellationTokenSource.Token); })
            {
                IsBackground = true
            };
            _stimulusProcessingThread.Start();
        }

        public new async Task<bool> Post(TStimulus stimulus)
        {
            await _stimulusChannel.Writer.WriteAsync(stimulus);
            return true;
        }

        public async Task<bool> PostAndWaitAsync(TStimulus stimulus, CancellationToken cancelToken = default)
        {
            await _stimulusChannel.Writer.WriteAsync(stimulus, cancelToken);
            await AwaitIdleAsync(cancelToken);
            return true;
        }

        public Task AwaitIdleAsync(CancellationToken cancelToken = default)
        {
            return Task.Run(() =>
            {
                while (_stimulusChannel.Reader.Count > 0)
                {
                    continue;
                }
                return true;
            });
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _internalThreadCancellationTokenSource.Cancel();
        }

        #region Private

        private async void ProcessStimuli(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    while (await _stimulusChannel.Reader.WaitToReadAsync(cancelToken))
                    {
                        var next = await _stimulusChannel.Reader.ReadAsync(cancelToken);
                        base.Post(next);
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
            }
        }

        #endregion
    }
}
