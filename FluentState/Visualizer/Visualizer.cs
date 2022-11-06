using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetGraph;
using DotNetGraph.Attributes;
using DotNetGraph.Core;
using DotNetGraph.Edge;
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
    private readonly IValidationResult<TState, TStimulus>? _validationResult;
    private readonly IActionRegistryValidation<TState, TStimulus> _enterActionRegistryValidation;
    private readonly IActionRegistryValidation<TState, TStimulus> _leaveActionRegistryValidation;

    public Visualizer(
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMap,
        IActionRegistryValidation<TState, TStimulus> enterActionRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation,
        IValidationResult<TState, TStimulus>? validationResult)
    {
        _stateMap = stateMap;
        _enterActionRegistryValidation = enterActionRegistryValidation;
        _leaveActionRegistryValidation = leaveActionRegistryValidation;
        _guardRegistryValidation = guardRegistryValidation;
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
        var graph = DoBuildGraph(stateMachineName);
        DoHighlightErrors(graph);

        return graph.Compile(indented: true);
    }

    #region Graph Building

    private DotGraph DoBuildGraph(string stateMachineName)
    {
        var graph = new DotGraph(stateMachineName)
        {
            Directed = true,
            Strict = true
        };
        
        var guarded_transitions = _guardRegistryValidation.GuardTransitions;
        var entry_actions = _enterActionRegistryValidation.TransitionActionTransitions;
        var leave_actions = _leaveActionRegistryValidation.TransitionActionTransitions;

        foreach (var node in _stateMap.TopLevelStates)
        {
            var state_node_id = $"{node}";

            var is_starting_node = node.Equals(_initialState);
            var state_node = graph.AddOrGetNode(state_node_id, ns =>
            {
                ns.Shape = is_starting_node ? DotNodeShape.Pentagon : DotNodeShape.Square;
                ns.Color = is_starting_node ? Color.Green : Color.Black;
            });

            foreach (var state_transition in _stateMap.StateTransitions(node))
            {
                var to_node_id = $"{state_transition.Value}";

                var transition = new Transition<TState, TStimulus>{From = node, To = state_transition.Value, Reason = state_transition.Key};

                if (guarded_transitions.Any(gt => gt.From.Equals(transition.From) && gt.To.Equals(transition.To) && gt.Reason.Equals(transition.Reason)))
                {
                    DoAddGuardedTransition(graph, transition);
                }
                else
                {
                    DoAddTransition(graph, transition);
                }
            }
        }

        return graph;
    }

    private void DoAddGuardedTransition(DotGraph graph, ITransition<TState, TStimulus> transition)
    {
        var guard_node_id = $"{transition.Reason}";
        var guard_node = graph.AddOrGetNode(guard_node_id, ns =>
        {
            ns.Shape = DotNodeShape.Diamond;
            ns.Color = Color.DarkOrange;
            ns.Label = $"{guard_node_id}?";
        });

        var from_node = graph.AddOrGetNode($"{transition.From}");
        var to_node = graph.AddOrGetNode($"{transition.To}");

        graph.AddEdge(from_node, guard_node, edge =>
        {
            edge.Label = $"{guard_node_id}";
        });

        graph.AddEdge(guard_node, to_node, edge =>
        {
            edge.Label = "Yes";
        });

        graph.AddEdge(guard_node, from_node, edge =>
        {
            edge.Label = "No";
        });
    }

    private void DoAddTransition(DotGraph graph, ITransition<TState, TStimulus> transition)
    {
        var from_node = graph.AddOrGetNode($"{transition.From}");
        var to_node = graph.AddOrGetNode($"{transition.To}");

        graph.AddEdge(from_node, to_node, edge =>
        {
            edge.Label = $"{transition.Reason}";
        });
    }

    private void DoHighlightErrors(DotGraph graph)
    {
        var errors = _validationResult?.Errors ?? Array.Empty<IValidationError<TState, TStimulus>>();
        foreach (var validation_error in errors)
        {
            foreach (var state in validation_error.ErrorStates)
            {
                var node = graph.GetNode($"{state}");
                if (node == null)
                {
                    throw new Exception(
                        "Could not find error node during visualization, please report this to the dev");
                }
                else
                {
                    node.FillColor = Color.Red;
                    node.Style = DotNodeStyle.Filled;
                }
            }

            foreach (var transition in validation_error.ErrorTransitions)
            {
                var from = graph.GetNode($"{transition.From}");
                if (from == null)
                {
                    throw new Exception(
                        "Could not find error node during visualization, please report this to the dev");
                }
                else
                {
                    from.FillColor = Color.Red;
                    from.Style = DotNodeStyle.Filled;
                }

                var to = graph.GetNode($"{transition.To}");
                if (to == null)
                {
                    throw new Exception(
                        "Could not find error node during visualization, please report this to the dev");
                }
                else
                {
                    to.FillColor = Color.Red;
                    to.Style = DotNodeStyle.Filled;
                }

                foreach (var edge in graph.Elements.OfType<DotEdge>())
                {
                    var left_node = edge.Left;
                    var right_node = edge.Right;

                    if (left_node == null || right_node == null)
                    {
                        continue;
                    }

                    if (left_node == from && right_node == to)
                    {
                        edge.Color = Color.Red;
                    }
                }
            }
        }
    }

    #endregion
}

public static class Extension
{
    public static DotNode? GetNode(this DotGraph graph, string id)
    {
        return graph.Elements.OfType<DotNode>().FirstOrDefault(e => e.Identifier == id);
    }

    public static DotNode AddOrGetNode(this DotGraph graph, string id, Action<DotNode>? nodeSetup = null)
    {
        var node = graph.GetNode(id);
        if (node != null)
        {
            nodeSetup?.Invoke(node);
            return node;
        }

        DotNode? n = null;
        graph.AddNode(id, node =>
        {
            n = node;
            nodeSetup?.Invoke(n);
        });

        if (n == null)
        {
            throw new Exception("If this happens, it's a bug within the dot generation library, sorry");
        }
        return n;
    }
}