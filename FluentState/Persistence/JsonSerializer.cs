using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluentState.Persistence
{
    public class JsonSerializer<TState, TStimulus> : ISerializer<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        private readonly IStateTypeConverter<TState> _stateTypeConverter;

        public JsonSerializer(IStateTypeConverter<TState> stateTypeConverter)
        {
            _stateTypeConverter = stateTypeConverter;
        }

        public async Task<bool> Save(IStateMachine<TState, TStimulus> machine, string path, CancellationToken cancellationToken=default)
        {
            var stateString = _stateTypeConverter.Convert(machine.CurrentState);

            var stateMachineJson = new JObject(new JProperty("state", stateString));

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
            return true;
        }
    }
}
