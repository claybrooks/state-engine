using FluentState.Extensions;
using FluentState.Persistence;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace FluentState.Config
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// {
    ///   "initialState": "Idle",
    ///   "enterActions": ["key1", "key2"],
    ///   "leaveActions": ["key3", "key4"],
    ///   "states": [
    ///     {
    ///       "name": "Idle",
    ///       "enterActions": ["key5"],
    ///       "leaveActions": ["key6"],
    ///       "transitions": [
    ///         {
    ///           "name": "Walking",
    ///           "stimulus": "Walk",
    ///           "enterActions": ["key7"],
    ///           "leaveActions": ["key8"],
    ///           "guards": ["key9"]
    ///         },
    ///         {
    ///           "name": "Running",
    ///           "stimulus": "Run"
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    /// </example>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TStimulus"></typeparam>
    public class JsonConfigLoader<TState, TStimulus> : IConfigLoader<TState, TStimulus>
        where TState : struct
        where TStimulus : struct
    {
        public TState InitialState { get; private set; }

        private IList<Action<TState, TState, TStimulus>> _globalEnterActions = new List<Action<TState, TState, TStimulus>>();
        private IList<Action<TState, TState, TStimulus>> _globalLeaveActions = new List<Action<TState, TState, TStimulus>>();
        private IList<StateConfig<TState, TStimulus>> _states = new List<StateConfig<TState, TStimulus>>();

        public JsonConfigLoader(
            string path,
            ITypeSerializer<TState> stateSerializer,
            ITypeSerializer<TStimulus> stimulusSerializer,
            IActionProvider<TState, TStimulus> actionProvider,
            IGuardProvider<TState, TStimulus> guardProvider)
        {
            Load(path, stateSerializer, stimulusSerializer, actionProvider, guardProvider);
        }

        public IEnumerable<Action<TState, TState, TStimulus>> GlobalEnterActions => _globalEnterActions;
        public IEnumerable<Action<TState, TState, TStimulus>> GlobalLeaveActions => _globalLeaveActions;
        public IEnumerable<StateConfig<TState, TStimulus>> States => _states;

        #region Private

        private void Load(
            string path,
            ITypeSerializer<TState> stateSerializer,
            ITypeSerializer<TStimulus> stimulusSerializer,
            IActionProvider<TState, TStimulus> actionProvider,
            IGuardProvider<TState, TStimulus> guardProvider)
        {
            var data = JObject.Parse(File.ReadAllText(path));

            LoadInitialState(data, stateSerializer);
            LoadGlobalActions(data, actionProvider);
            LoadStates(data, stateSerializer, stimulusSerializer, actionProvider, guardProvider);
        }

        private void LoadInitialState(JObject data, ITypeSerializer<TState> stateSerializer)
        {
            var initialState = stateSerializer.Convert(data.Value<string>("initialState") ?? "");
            if (initialState == null)
            {
                throw new InvalidDataException($"Unable to parse initial state from {data}");
            }

            InitialState = initialState.Value;
        }

        private void LoadGlobalActions(JObject data, IActionProvider<TState, TStimulus> actionProvider)
        {
            var enterActions = data.NonNullableValues<string>("enterActions");
            var leaveActions = data.NonNullableValues<string>("leaveActions");

            if (enterActions != null)
            {
                foreach (var actionKey in enterActions)
                {
                    var action = actionProvider.Get(actionKey);
                    if (action == null)
                    {
                        throw new InvalidDataException($"Unable to get global state machine enter action with key {actionKey}");
                    }
                    _globalEnterActions.Add(action);
                }
            }

            if (leaveActions != null)
            {
                foreach (var actionKey in leaveActions)
                {
                    var action = actionProvider.Get(actionKey);
                    if (action == null)
                    {
                        throw new InvalidDataException($"Unable to get global state machine leave action with key {actionKey}");
                    }
                    _globalLeaveActions.Add(action);
                }
            }
        }

        private void LoadStates(JObject data,
            ITypeSerializer<TState> stateSerializer,
            ITypeSerializer<TStimulus> stimulusSerializer,
            IActionProvider<TState, TStimulus> actionProvider,
            IGuardProvider<TState, TStimulus> guardProvider)
        {
            var states = data.NonNullableValues<JObject>("states");

            if (states != null)
            {
                foreach (var state in states)
                {
                    _states.Add(LoadState(state, stateSerializer, stimulusSerializer, actionProvider, guardProvider));
                }
            }
        }

        private StateConfig<TState, TStimulus> LoadState(
            JToken data,
            ITypeSerializer<TState> stateSerializer,
            ITypeSerializer<TStimulus> stimulusSerializer,
            IActionProvider<TState, TStimulus> actionProvider,
            IGuardProvider<TState, TStimulus> guardProvider
        )
        {
            var state = stateSerializer.Convert(data["name"]?.Value<string>() ?? "");
            if (state == null)
            {
                throw new InvalidDataException($"Unable to retrieve state from {data}");
            }

            var stateConfig = new StateConfig<TState, TStimulus>(state.Value);

            var enterActions = data.NonNullableValues<string>("enterActions");
            var leaveActions = data.NonNullableValues<string>("leaveActions");
            var transitions = data.NonNullableValues<JObject>("transitions");

            if (enterActions != null)
            {
                foreach (var actionKey in enterActions)
                {
                    stateConfig.EnterActions.Add(actionProvider.Get(actionKey));
                }
            }

            if (leaveActions != null)
            {
                foreach (var actionKey in leaveActions)
                {
                    stateConfig.LeaveActions.Add(actionProvider.Get(actionKey));
                }
            }

            if (transitions != null)
            {
                foreach (var transition in transitions)
                {
                    stateConfig.Transitions.Add(LoadTransition(transition, stateSerializer, stimulusSerializer, actionProvider, guardProvider));
                }
            }

            return stateConfig;
        }

        private TransitionConfig<TState, TStimulus> LoadTransition(
            JToken data,
            ITypeSerializer<TState> stateSerializer,
            ITypeSerializer<TStimulus> stimulusSerializer,
            IActionProvider<TState, TStimulus> actionProvider,
            IGuardProvider<TState, TStimulus> guardProvider)
        {
            var state = stateSerializer.Convert(data.Value<string>("name") ?? "");
            var stimulus = stimulusSerializer.Convert(data.Value<string>("stimulus") ?? "");
            if (state == null)
            {
                throw new InvalidDataException($"Unable to retrieve state from {data}");
            }
            if (stimulus == null)
            {
                throw new InvalidDataException($"Unable to retrieve stimulus from {data}");
            }

            var transitionConfig = new TransitionConfig<TState, TStimulus>(state.Value, stimulus.Value);

            var enterActions = data.NonNullableValues<string>("enterActions");
            var leaveActions = data.NonNullableValues<string>("leaveActions");
            var guards = data.NonNullableValues<string>("guards");

            if (enterActions != null)
            {
                foreach (var actionKey in enterActions)
                {
                    transitionConfig.EnterActions.Add(actionProvider.Get(actionKey));
                }
            }

            if (leaveActions != null)
            {
                foreach (var actionKey in leaveActions)
                {
                    transitionConfig.LeaveActions.Add(actionProvider.Get(actionKey));
                }
            }

            if (guards != null)
            {
                foreach (var guard in guards)
                {
                    transitionConfig.Guards.Add(guardProvider.Get(guard));
                }
            }

            return transitionConfig;
        }

        #endregion
    }
}
