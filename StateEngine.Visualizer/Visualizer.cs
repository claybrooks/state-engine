using DotNetGraph.Core;
using System.Drawing;
using System.Text;
using DotNetGraph.Compilation;
using DotNetGraph.Extensions;

namespace StateEngine.Visualizer;

public sealed class VisualizerFactory<TState, TStimulus> : IVisualizerFactory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public IVisualizer CreateVisualizer(VisualizationRules<TState, TStimulus> rules, TState initialState, IStateMapValidation<TState, TStimulus> stateMap,
        ITransitionActionRegistryValidation<TState, TStimulus> enterActionRegistryValidation, ITransitionActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        return new Visualizer<TState, TStimulus>(rules, initialState, stateMap, enterActionRegistryValidation, leaveActionRegistryValidation, guardRegistryValidation);
    }
}

internal abstract class AbstractVisualizer : IVisualizer
{
    private static long _nodeCounter;
    protected static long NodeCounter => ++_nodeCounter;

    public abstract Task CreateDotAsync(string stateMachineName, string fullPathToOutputFile);
    public abstract Task<string> CreateDotAsync(string stateMachineName);
}

internal sealed class Visualizer<TState, TStimulus> : AbstractVisualizer
    where TState : struct
    where TStimulus : struct
{
    private readonly VisualizationRules<TState, TStimulus> _rules;
    private readonly TState _initialState;
    private readonly IStateMapValidation<TState, TStimulus> _stateMap;
    private readonly ITransitionGuardRegistryValidation<TState, TStimulus> _guardRegistryValidation;
    private readonly ITransitionActionRegistryValidation<TState, TStimulus> _enterActionRegistryValidation;
    private readonly ITransitionActionRegistryValidation<TState, TStimulus> _leaveActionRegistryValidation;


    public Visualizer(
        VisualizationRules<TState, TStimulus> rules,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMap,
        ITransitionActionRegistryValidation<TState, TStimulus> enterActionRegistryValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation)
    {
        _rules = rules;
        _stateMap = stateMap;
        _enterActionRegistryValidation = enterActionRegistryValidation;
        _leaveActionRegistryValidation = leaveActionRegistryValidation;
        _guardRegistryValidation = guardRegistryValidation;
        _initialState = initialState;
    }

    public override async Task CreateDotAsync(string stateMachineName, string fullPathToOutputFile)
    {
        var data = await CreateDotAsync(stateMachineName);
        File.WriteAllText(fullPathToOutputFile, data);
    }

    public override async Task<string> CreateDotAsync(string stateMachineName)
    {
        var graph = DoBuildGraph(stateMachineName);
        if (_rules.ValidationResults != null)
        {
            DoHighlightErrors(graph, _rules.ValidationResults);
        }

        var string_writer = new StringWriter();
        await graph.CompileAsync(new CompilationContext(string_writer, new CompilationOptions()
        {
            Indented = true
        }));

        return string_writer.ToString();
    }

    #region Graph Building

    private DotGraph DoBuildGraph(string stateMachineName)
    {
        var graph = new DotGraph()
        {
            Directed = true,
            Strict = true,
            Label = stateMachineName,
            Identifier = new DotIdentifier(stateMachineName)
        };

        foreach (var node in _stateMap.TopLevelStates)
        {
            var state_node_id = $"{node}";

            var is_starting_node = node.Equals(_initialState);
            graph.AddOrGetNode(state_node_id, ns =>
            {
                ns.Shape = is_starting_node ? DotNodeShape.Pentagon : DotNodeShape.Square;
                ns.Color = is_starting_node ? DotColor.Green : DotColor.Black;
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
        VisualizationRules<TState, TStimulus> rules,
        ITransition<TState, TStimulus> transition,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardedGuardRegistryValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> entryActionRegistryValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation)
    {
        var from_node_id = $"{transition.From}";
        var to_node_id = $"{transition.To}";

        graph.AddOrGetNode(from_node_id, ns =>
        {
            ns.Color = DotColor.Green;
        });
        graph.AddOrGetNode(to_node_id, ns =>
        {
            ns.Color = DotColor.Green;
        });

        var state_wide_leave_actions = leaveActionRegistryValidation.StateWideActions.GetValueOrDefault(transition.From, Enumerable.Empty<string>());
        var state_wide_enter_actions = entryActionRegistryValidation.StateWideActions.GetValueOrDefault(transition.From, Enumerable.Empty<string>());

        var transition_leave_actions = leaveActionRegistryValidation.ActionsOnTransition.GetValueOrDefault(transition, Enumerable.Empty<string>());
        var transition_enter_actions = entryActionRegistryValidation.ActionsOnTransition.GetValueOrDefault(transition, Enumerable.Empty<string>());
        
        var last_action_node = from_node_id;
        var last_action_label = from_node_id;
        var reason = $"{transition.Reason}";

        // First check for guard
        if (guardedGuardRegistryValidation.GuardedTransitions.Any(gt => gt.From.Equals(transition.From) && gt.To.Equals(transition.To) && gt.Reason.Equals(transition.Reason)))
        {
            var guard_node_id = $"{transition.Reason}:{NodeCounter}";
            var guard_node_label = $"{transition.Reason}";
            DoAddTransition(graph, last_action_node, last_action_label, guard_node_id, $"{guard_node_label}?", reason);
            last_action_node = guard_node_id;
            last_action_label = guard_node_label;

            DoAddTransition(graph, last_action_node, last_action_label, from_node_id, from_node_id, "No");

            reason = "Yes";
            graph.AddOrGetNode(guard_node_id, ns =>
            {
                ns.Shape = DotNodeShape.Diamond;
                ns.Color = DotColor.OrangeRed;
            });
        }

        var action_node_shape = DotNodeShape.Rectangle;
        var action_node_color = DotColor.Blue;

        if (rules.DisplayActions)
        {
            foreach (var global_leave_action in leaveActionRegistryValidation.GlobalActions)
            {
                var global_leave_action_node = $"{global_leave_action}:{NodeCounter}";
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
                var state_wide_leave_action_node = $"{state_wide_leave_action}:{NodeCounter}";
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
                var transition_leave_action_node = $"{transition_leave_action}:{NodeCounter}";
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
                var global_enter_action_node = $"{global_enter_action}:{NodeCounter}";
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
                var state_wide_enter_action_node = $"{state_wide_enter_action}:{NodeCounter}";
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
                var transition_enter_action_node = $"{transition_enter_action}:{NodeCounter}";
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

        graph.Add(new DotEdge()
        {
            From = from_node.Identifier,
            To = to_node.Identifier,
            Label = reason != null ? $"{reason}" : ""
        });
    }

    private static void DoHighlightErrors(DotGraph graph, IValidationResult<TState, TStimulus> validationResult)
    {
        var errors = validationResult.Errors;
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
                    node.FillColor = DotColor.Red;
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
                    from.FillColor = DotColor.Red;
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
                    to.FillColor = DotColor.Red;
                    to.Style = DotNodeStyle.Filled;
                }

                foreach (var edge in graph.Elements.OfType<DotEdge>())
                {
                    var left_node = edge.From;
                    var right_node = edge.To;

                    if (left_node == null || right_node == null)
                    {
                        continue;
                    }

                    if (left_node == from.Identifier && right_node == to.Identifier)
                    {
                        edge.Color = DotColor.Red;
                    }
                }
            }
        }
    }

    #endregion
}

internal static class Extension
{
    public static DotNode? GetNode(this DotGraph graph, string id, Action<DotNode>? nodeSetup = null)
    {
        var node = graph.Elements.OfType<DotNode>().FirstOrDefault(e => e.Identifier.Value == id);
        if (node != null && nodeSetup != null)
        {
            nodeSetup(node);
        }

        return node;
    }

    public static DotNode AddOrGetNode(this DotGraph graph, string id, Action<DotNode>? nodeSetup = null)
    {
        var node = graph.GetNode(id);
        if (node != null)
        {
            nodeSetup?.Invoke(node);
            return node;
        }

        var n = new DotNode()
        {
            Identifier = new DotIdentifier(id)
        };
        nodeSetup?.Invoke(n);
        graph.Add(n);

        return n;
    }
}