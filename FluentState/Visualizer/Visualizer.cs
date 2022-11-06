using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;

namespace FluentState;

public interface IVisualizer
{
    Task CreateDot(string stateMachineName, string fullPathToOutputFile, CancellationToken token = default);

    string CreateDot(string stateMachineName);
}

public class VisualizationRules
{
    public bool DisplayActions { get; set; } = true;
}


internal sealed class Visualizer<TState, TStimulus> : IVisualizer
    where TState : struct
    where TStimulus : struct
{
    private readonly VisualizationRules _rules;
    private readonly TState _initialState;
    private readonly IStateMapValidation<TState, TStimulus> _stateMap;
    private readonly IGuardRegistryValidation<TState, TStimulus> _guardRegistryValidation;
    private readonly IValidationResult<TState, TStimulus>? _validationResult;
    private readonly IActionRegistryValidation<TState, TStimulus> _enterActionRegistryValidation;
    private readonly IActionRegistryValidation<TState, TStimulus> _leaveActionRegistryValidation;

    public Visualizer(
        VisualizationRules rules,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMap,
        IActionRegistryValidation<TState, TStimulus> enterActionRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation,
        IValidationResult<TState, TStimulus>? validationResult)
    {
        _rules = rules;
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
        foreach (var node in _stateMap.TopLevelStates)
        {
            var state_node_id = $"{node}";

            var is_starting_node = node.Equals(_initialState);
            graph.AddOrGetNode(state_node_id, ns =>
            {
                ns.Shape = is_starting_node ? DotNodeShape.Pentagon : DotNodeShape.Square;
                ns.Color = is_starting_node ? Color.Green : Color.Black;
            });

            foreach (var state_transition in _stateMap.StateTransitions(node))
            {
                var transition = new Transition<TState, TStimulus>{From = node, To = state_transition.Value, Reason = state_transition.Key};
                DoAddTransition(graph, _rules, transition, _guardRegistryValidation, _enterActionRegistryValidation, _leaveActionRegistryValidation);
            }
        }

        return graph;
    }

    private static void DoAddTransition(DotGraph graph,
        VisualizationRules rules,
        ITransition<TState, TStimulus> transition,
        IGuardRegistryValidation<TState, TStimulus> guardedGuardRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> entryActionRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation)
    {
        var from_node_id = $"{transition.From}";
        var to_node_id = $"{transition.To}";

        var state_wide_leave_actions = leaveActionRegistryValidation.StateWideActions.GetValueOrDefault(transition.From, Enumerable.Empty<string>());
        var state_wide_enter_actions = entryActionRegistryValidation.StateWideActions.GetValueOrDefault(transition.From, Enumerable.Empty<string>());

        var transition_leave_actions = leaveActionRegistryValidation.ActionsOnTransition.GetValueOrDefault(transition, Enumerable.Empty<string>());
        var transition_enter_actions = entryActionRegistryValidation.ActionsOnTransition.GetValueOrDefault(transition, Enumerable.Empty<string>());
        
        var last_action_node = from_node_id;
        var last_action_label = from_node_id;
        var reason = $"{transition.Reason}";

        // First check for guard
        if (guardedGuardRegistryValidation.GuardTransitions.Any(gt => gt.From.Equals(transition.From) && gt.To.Equals(transition.To) && gt.Reason.Equals(transition.Reason)))
        {
            var guard_node_id = $"{transition.Reason}:{Guid.NewGuid()}";
            var guard_node_label = $"{transition.Reason}";
            DoAddTransition(graph, last_action_node, last_action_label, guard_node_id, $"{guard_node_label}?", reason);
            reason = null;
            last_action_node = guard_node_id;
            last_action_label = guard_node_label;

            DoAddTransition(graph, last_action_node, last_action_label, from_node_id, from_node_id, "No");

            reason = "Yes";
            graph.AddOrGetNode(guard_node_id, ns =>
            {
                ns.Shape = DotNodeShape.Diamond;
                ns.Color = Color.DarkOrange;
            });
        }

        var action_node_shape = DotNodeShape.Rectangle;
        var action_node_color = Color.Blue;

        if (rules.DisplayActions)
        {
            foreach (var global_leave_action in leaveActionRegistryValidation.GlobalActions)
            {
                var global_leave_action_node = $"{global_leave_action}:{Guid.NewGuid()}";
                DoAddTransition(graph, last_action_node, last_action_label, global_leave_action_node, global_leave_action, reason);
                reason = null;
                last_action_node = global_leave_action_node;
                last_action_label = global_leave_action;

                graph.AddOrGetNode(last_action_node, ns =>
                {
                    ns.Color = action_node_color;
                    ns.Shape = action_node_shape;
                });
            }

            foreach (var state_wide_leave_action in state_wide_leave_actions)
            {
                var state_wide_leave_action_node = $"{state_wide_leave_action}:{Guid.NewGuid()}";
                DoAddTransition(graph, last_action_node, last_action_label, state_wide_leave_action_node, state_wide_leave_action, reason);
                reason = null;
                last_action_node = state_wide_leave_action_node;
                last_action_label = state_wide_leave_action;

                graph.AddOrGetNode(last_action_node, ns =>
                {
                    ns.Color = action_node_color;
                    ns.Shape = action_node_shape;
                });
            }

            foreach (var transition_leave_action in transition_leave_actions)
            {
                var transition_leave_action_node = $"{transition_leave_action}:{Guid.NewGuid()}";
                DoAddTransition(graph, last_action_node, last_action_label, transition_leave_action_node, transition_leave_action, reason);
                reason = null;
                last_action_node = transition_leave_action_node;
                last_action_label = transition_leave_action;

                graph.AddOrGetNode(last_action_node, ns =>
                {
                    ns.Color = action_node_color;
                    ns.Shape = action_node_shape;
                });
            }

            foreach (var global_enter_action in entryActionRegistryValidation.GlobalActions)
            {
                var global_enter_action_node = $"{global_enter_action}:{Guid.NewGuid()}";
                DoAddTransition(graph, last_action_node, last_action_label, global_enter_action_node, global_enter_action, reason);
                reason = null;
                last_action_node = global_enter_action_node;
                last_action_label = global_enter_action;

                graph.AddOrGetNode(last_action_node, ns =>
                {
                    ns.Color = action_node_color;
                    ns.Shape = action_node_shape;
                });
            }

            foreach (var state_wide_enter_action in state_wide_enter_actions)
            {
                var state_wide_enter_action_node = $"{state_wide_enter_action}:{Guid.NewGuid()}";
                DoAddTransition(graph, last_action_node, last_action_label, state_wide_enter_action_node, state_wide_enter_action, reason);
                reason = null;
                last_action_node = state_wide_enter_action_node;
                last_action_label = state_wide_enter_action;

                graph.AddOrGetNode(last_action_node, ns =>
                {
                    ns.Color = action_node_color;
                    ns.Shape = action_node_shape;
                });
            }

            foreach (var transition_enter_action in transition_enter_actions)
            {
                var transition_enter_action_node = $"{transition_enter_action}:{Guid.NewGuid()}";
                DoAddTransition(graph, last_action_node, last_action_label, transition_enter_action_node, transition_enter_action, reason);
                reason = null;
                last_action_node = transition_enter_action_node;
                last_action_label = transition_enter_action;

                graph.AddOrGetNode(last_action_node, ns =>
                {
                    ns.Color = action_node_color;
                    ns.Shape = action_node_shape;
                });
            }
        }

        DoAddTransition(graph, last_action_node, last_action_label, to_node_id, to_node_id, reason);
    }

    private static void DoAddTransition(DotGraph graph, string fromId, string fromLabel, string toId, string toLabel, string? reason = null)
    {
        var from_node = graph.AddOrGetNode(fromId, ns =>
        {
            ns.Label = fromLabel;
        });

        var to_node = graph.AddOrGetNode(toId, ns =>
        {
            ns.Label = toLabel;
        });

        graph.AddEdge(from_node, to_node, edge =>
        {
            edge.Label = reason != null ? $"{reason}" : edge.Label;
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