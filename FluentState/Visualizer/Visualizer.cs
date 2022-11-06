using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetGraph;
using DotNetGraph.Extensions;
using DotNetGraph.Node;

namespace FluentState;

public interface IVisualizer
{
    Task CreateDot(string stateMachineName, string fullPathToOutputFile, CancellationToken token = default);

    string CreateDot(string stateMachineName);
}

internal sealed class Visualizer<TState, TStimulus> : IVisualizer
    where TState : struct
    where TStimulus : struct
{
    private readonly TState _initialState;
    private readonly IStateMapValidation<TState, TStimulus> _stateMap;
    private readonly IGuardRegistryValidation<TState, TStimulus> _guardRegistryValidation;
    private readonly IValidationResult? _validationResult;

    public Visualizer(
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMap,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation,
        IValidationResult? validationResult)
    {
        this._stateMap = stateMap;
        this._guardRegistryValidation = guardRegistryValidation;
        _validationResult = validationResult;
        _initialState = initialState;
    }

    public async Task CreateDot(string stateMachineName, string fullPathToOutputFile, CancellationToken token = default)
    {
        var data = CreateDot(stateMachineName);
        await File.WriteAllTextAsync(fullPathToOutputFile, data, token);
    }

    public string CreateDot(string stateMachineName)
    {
        var graph = new DotGraph(stateMachineName)
        {
            Directed = true,
            Strict = true
        };

        var nodes = _stateMap.TopLevelStates;
        var guarded_transitions = _guardRegistryValidation.GuardTransitions;

        foreach (var node in nodes)
        {
            var graph_node = $"{node}";
            if (node.Equals(_initialState))
            {
                graph.AddNodeIfNotExist(graph_node, ns =>
                {
                    ns.Shape = DotNodeShape.Rectangle;
                });
            }

            foreach (var transition in _stateMap.StateTransitions(node))
            {
                var to_node = $"{transition.Value}";

                var transition_type = new Transition<TState, TStimulus>{From = node, To = transition.Value, Reason = transition.Key};

                if (guarded_transitions.Any(gt => gt.From.Equals(transition_type.From) && gt.To.Equals(transition_type.To) && gt.Reason.Equals(transition_type.Reason)))
                {
                    var guard_node = $"{transition.Key}";
                    graph.AddNodeIfNotExist(guard_node, ns =>
                    {
                        ns.Shape = DotNodeShape.Diamond;
                        ns.Label = $"Allow {guard_node}?";
                    });

                    graph.AddEdge(graph_node, guard_node, edge =>
                    {
                        edge.Label = $"{guard_node}";
                    });

                    graph.AddNodeIfNotExist($"{to_node}");

                    graph.AddEdge(guard_node, to_node, edge =>
                    {
                        edge.Label = "Yes";
                    });

                    graph.AddEdge(guard_node, graph_node, edge =>
                    {
                        edge.Label = "No";
                    });
                }
                else
                {
                    graph.AddNodeIfNotExist(to_node);
                    graph.AddEdge(graph_node, to_node, edge =>
                    {
                        edge.Label = $"{transition.Key}";
                    });
                }
            }
        }
        
        return graph.Compile(indented: true);
    }
}

public static class Extension
{
    public static DotGraph AddNodeIfNotExist(this DotGraph graph, string id, Action<DotNode>? nodeSetup = null)
    {
        if (graph.Elements.OfType<DotNode>().All(dn => dn.Identifier != id))
        {
            graph.AddNode(id, nodeSetup);
        }

        return graph;
    }
}