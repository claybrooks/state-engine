using System.Threading.Channels;

namespace FluentState
{
    public class AsyncStateMachine<TState, TStimulus> : StateMachine<TState, TStimulus>, IAsyncStateMachine<TState, TStimulus>
        where TState : notnull
        where TStimulus : notnull
    {
        private readonly Channel<TStimulus> _stimulusChannel = Channel.CreateUnbounded<TStimulus>();
        private readonly Thread _stimulusProcessingThread;
        private CancellationTokenSource _cancellationTokenSource;

        public AsyncStateMachine(TState initialState) : base(initialState)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _stimulusProcessingThread = new Thread(() => { ProcessStimuli(_cancellationTokenSource.Token); });
            _stimulusProcessingThread.IsBackground = true;
            _stimulusProcessingThread.Start();
        }

        public new async Task<bool> Post(TStimulus stimulus)
        {
            await _stimulusChannel.Writer.WriteAsync(stimulus);
            return true;
        }

        public async Task<bool> PostAndWaitAsync(TStimulus stimulus)
        {
            await _stimulusChannel.Writer.WriteAsync(stimulus);
            await AwaitIdleAsync();
            return true;
        }

        public async Task AwaitIdleAsync()
        {
            await _stimulusChannel.Reader.Completion;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        #region Private

        private async void ProcessStimuli(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                IAsyncEnumerable<TStimulus> items;
                try
                {
                    await _stimulusChannel.Reader.WaitToReadAsync(cancelToken);
                    items = _stimulusChannel.Reader.ReadAllAsync(cancelToken);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }

                if (items != null)
                {
                    await foreach (var item in items)
                    {
                        base.Post(item);
                    }
                }
            }
        }

        #endregion
    }
}
