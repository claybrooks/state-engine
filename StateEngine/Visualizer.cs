﻿namespace StateEngine;

public interface IVisualizerFactory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IVisualizer CreateVisualizer(
        VisualizationRules<TState, TStimulus> rules,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMap,
        ITransitionActionRegistryValidation<TState, TStimulus> enterActionRegistryValidation,
        ITransitionActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation,
        ITransitionGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);
}

public interface IVisualizer
{
    Task CreateDotAsync(string stateMachineName, string fullPathToOutputFile);

    Task<string> CreateDotAsync(string stateMachineName);
}

public class VisualizationRules<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public bool DisplayActions { get; set; } = true;
    public IValidationResult<TState, TStimulus>? ValidationResults { get; set; } = null;
}
