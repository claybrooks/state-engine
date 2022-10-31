using FluentState.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace FluentState.Config;

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

    private readonly IList<Action<TState, TState, TStimulus>> _globalEnterActions = new List<Action<TState, TState, TStimulus>>();
    private readonly IList<Action<TState, TState, TStimulus>> _globalLeaveActions = new List<Action<TState, TState, TStimulus>>();
    private readonly IList<StateConfig<TState, TStimulus>> _states = new List<StateConfig<TState, TStimulus>>();

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
        var initial_state = stateSerializer.Convert(data.Value<string>("initialState") ?? "");
        if (initial_state == null)
        {
            throw new InvalidDataException($"Unable to parse initial state from {data}");
        }

        InitialState = initial_state.Value;
    }

    private void LoadGlobalActions(JToken data, IActionProvider<TState, TStimulus> actionProvider)
    {
        var enter_actions = data.NonNullableValues<string>("enterActions");
        var leave_actions = data.NonNullableValues<string>("leaveActions");

        if (enter_actions != null)
        {
            foreach (var action_key in enter_actions)
            {
                var action = actionProvider.Get(action_key);
                if (action == null)
                {
                    throw new InvalidDataException($"Unable to get global state machine enter action with key {action_key}");
                }
                _globalEnterActions.Add(action);
            }
        }

        if (leave_actions != null)
        {
            foreach (var action_key in leave_actions)
            {
                var action = actionProvider.Get(action_key);
                if (action == null)
                {
                    throw new InvalidDataException($"Unable to get global state machine leave action with key {action_key}");
                }
                _globalLeaveActions.Add(action);
            }
        }
    }

    private void LoadStates(JToken data,
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

        var state_config = new StateConfig<TState, TStimulus>(state.Value);

        var enter_actions = data.NonNullableValues<string>("enterActions");
        var leave_actions = data.NonNullableValues<string>("leaveActions");
        var transitions = data.NonNullableValues<JObject>("transitions");

        if (enter_actions != null)
        {
            foreach (var action_key in enter_actions)
            {
                state_config.EnterActions.Add(actionProvider.Get(action_key));
            }
        }

        if (leave_actions != null)
        {
            foreach (var action_key in leave_actions)
            {
                state_config.LeaveActions.Add(actionProvider.Get(action_key));
            }
        }

        if (transitions != null)
        {
            foreach (var transition in transitions)
            {
                state_config.Transitions.Add(LoadTransition(transition, stateSerializer, stimulusSerializer, actionProvider, guardProvider));
            }
        }

        return state_config;
    }

    private static TransitionConfig<TState, TStimulus> LoadTransition(
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

        var transition_config = new TransitionConfig<TState, TStimulus>(state.Value, stimulus.Value);

        var enter_actions = data.NonNullableValues<string>("enterActions");
        var leave_actions = data.NonNullableValues<string>("leaveActions");
        var guards = data.NonNullableValues<string>("guards");

        if (enter_actions != null)
        {
            foreach (var action_key in enter_actions)
            {
                transition_config.EnterActions.Add(actionProvider.Get(action_key));
            }
        }

        if (leave_actions != null)
        {
            foreach (var action_key in leave_actions)
            {
                transition_config.LeaveActions.Add(actionProvider.Get(action_key));
            }
        }

        if (guards != null)
        {
            foreach (var guard in guards)
            {
                transition_config.Guards.Add(guardProvider.Get(guard));
            }
        }

        return transition_config;
    }

    #endregion
}