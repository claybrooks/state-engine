namespace StateEngine;

public interface IVisualizerFactory<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    IVisualizer CreateVisualizer(
        VisualizationRules<TState, TStimulus> rules,
        TState initialState,
        IStateMapValidation<TState, TStimulus> stateMap,
        IActionRegistryValidation<TState, TStimulus> enterActionRegistryValidation,
        IActionRegistryValidation<TState, TStimulus> leaveActionRegistryValidation,
        IGuardRegistryValidation<TState, TStimulus> guardRegistryValidation);
}

public interface IVisualizer
{
    void CreateDot(string stateMachineName, string fullPathToOutputFile);

    string CreateDot(string stateMachineName);
}

public class VisualizationRules<TState, TStimulus>
    where TState : struct
    where TStimulus : struct
{
    public bool DisplayActions { get; set; } = true;
    public IValidationResult<TState, TStimulus>? ConsiderValidationResults { get; set; } = null;
}
