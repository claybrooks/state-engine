using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentState.MachineParts;

namespace FluentState.Visualizer;

public interface IVisualizer
{
    Task CreateDot(string stateMachineName, string fullPathToOutputFile, CancellationToken token = default);

    string CreateDot(string stateMachineName);
}

public class Visualizer<TState, TStimulus> : IVisualizer
    where TState : struct
    where TStimulus : struct
{
    private readonly TState _initialState;
    private readonly IStateMapValidation<TState, TStimulus> _stateMap;
    private readonly IStateGuardValidation<TState, TStimulus> _guardValidation;

    public Visualizer(TState initialState, IStateMapValidation<TState, TStimulus> stateMap,
        IStateGuardValidation<TState, TStimulus> guardValidation)
    {
        this._stateMap = stateMap;
        this._guardValidation = guardValidation;
        _initialState = initialState;
    }

    public async Task CreateDot(string stateMachineName, string fullPathToOutputFile, CancellationToken token = default)
    {
        var data = CreateDot(stateMachineName);
        await File.WriteAllTextAsync(fullPathToOutputFile, data, token);
    }

    public string CreateDot(string stateMachineName)
    {
        var data = $"digraph {stateMachineName} {{\n";

        var nodes = _stateMap.TopLevelStates;
        var guarded_transitions = _guardValidation.GuardTransitions;

        foreach (var node in nodes)
        {
            if (node.Equals(_initialState))
            {
                data += $"{node} [ shape=rectangle style=filled fillcolor=green ]\n";
            }
            else
            {
                data += $"{node}\n";
            }

            foreach (var transition in _stateMap.StateTransitions(node))
            {
                var transition_type = new Transition<TState, TStimulus>{From = node, To = transition.Value, Reason = transition.Key};

                if (guarded_transitions.Any(gt => gt.From.Equals(transition_type.From) && gt.To.Equals(transition_type.To) && gt.Reason.Equals(transition_type.Reason)))
                {
                    data += $"{transition.Key} [ shape=diamond label=\"Allow {transition.Key}?\" ]\n";
                    data += $"{node} -> {transition.Key} [ label=\"{transition.Key}\" ]\n";
                    data += $"{transition.Key} -> {node} [ label=No color=red ]\n";
                    data += $"{transition.Key} -> {transition.Value} [ label=Yes ]\n";
                }
                else
                {
                    data += $"{node} -> {transition.Value} [ label=\"{transition.Key}\" ]\n";
                }
            }
        }

        data += "}";
        return data;
    }
}