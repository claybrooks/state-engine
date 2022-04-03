using FluentState.History;
using FluentState.Machine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluentState.Persistence
{
    public class DefaultJsonSerializer<TState, TStimulus> : IStateMachineSerializer<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        private readonly ITypeSerializer<TState> _stateTypeConverter;
        private readonly ITypeSerializer<TStimulus> _stimulusTypeConverter;

        public DefaultJsonSerializer(ITypeSerializer<TState> stateTypeConverter, ITypeSerializer<TStimulus> stimulusTypeConverter)
        {
            _stateTypeConverter = stateTypeConverter;
            _stimulusTypeConverter = stimulusTypeConverter;
        }

        public async Task<bool> Save(IStateMachine<TState, TStimulus> machine, string path, CancellationToken cancellationToken=default)
        {
            var stateString = _stateTypeConverter.Convert(machine.CurrentState);

            var stateMachineJson = new JObject(
                new JProperty("state", stateString),
                new JProperty("history", GetHistory(machine))
            );

            await File.WriteAllTextAsync(path, stateMachineJson.ToString(), cancellationToken);
            return true;
        }

        public async Task<bool> Load(IStateMachine<TState, TStimulus> machine, string path, CancellationToken cancellationToken=default)
        {
            var data = JObject.Parse(await File.ReadAllTextAsync(path, cancellationToken));

            var stateString = data.Value<string>("state");
            if (stateString == null)
            {
                return false;
            }

            TState? state = _stateTypeConverter.Convert(stateString);
            if (state == null)
            {
                return false;
            }

            machine.OverrideState(state.Value);

            var historyData = data.Value<JArray>("history");
            if (historyData != null)
            {
                machine.History.Clear();
                foreach (var item in historyData)
                {
                    var enteringStateString = item.Value<string>(nameof(HistoryItem<TState, TStimulus>.EnteringState));
                    var leavingStateString = item.Value<string>(nameof(HistoryItem<TState, TStimulus>.LeavingState));
                    var reasonString = item.Value<string>(nameof(HistoryItem<TState, TStimulus>.Reason));
                    var when = item.Value<DateTime>(nameof(HistoryItem<TState, TStimulus>.When));

                    if (enteringStateString == null || leavingStateString == null || reasonString == null)
                    {
                        return false;
                    }

                    TState? enteringState = _stateTypeConverter.Convert(enteringStateString);
                    TState? leavingState = _stateTypeConverter.Convert(leavingStateString);
                    TStimulus? reason = _stimulusTypeConverter.Convert(reasonString);

                    if (enteringState == null || leavingState == null || reason == null)
                    {
                        return false;
                    }

                    machine.History.Add(new HistoryItem<TState, TStimulus>()
                    {
                        EnteringState= enteringState.Value,
                        LeavingState= leavingState.Value,
                        Reason= reason.Value,
                        When= when,
                    });
                }
            }

            return true;
        }

        #region Private

        IEnumerable<JToken> GetHistory(IStateMachine<TState, TStimulus> machine)
        {
            var copiedItems = new List<JToken>();
            foreach(var historyItem in machine.History)
            {
                copiedItems.Add(new JObject(
                    new JProperty(nameof(HistoryItem<TState, TStimulus>.EnteringState), _stateTypeConverter.Convert(historyItem.EnteringState)),
                    new JProperty(nameof(HistoryItem<TState, TStimulus>.LeavingState), _stateTypeConverter.Convert(historyItem.LeavingState)),
                    new JProperty(nameof(HistoryItem<TState, TStimulus>.Reason), _stimulusTypeConverter.Convert(historyItem.Reason)),
                    new JProperty(nameof(HistoryItem<TState, TStimulus>.When), historyItem.When)
                ));
            }
            return copiedItems;
        }

        #endregion
    }
}
