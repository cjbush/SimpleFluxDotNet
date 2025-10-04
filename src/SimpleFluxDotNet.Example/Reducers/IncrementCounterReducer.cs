using SimpleFluxDotNet.Abstractions;
using SimpleFluxDotNet.Example.Actions;
using SimpleFluxDotNet.Example.States;

namespace SimpleFluxDotNet.Example.Reducers;

public sealed class IncrementCounterReducer : IFluxReducer<ExampleState, IncrementCounterButtonClickedAction>
{
    public ExampleState Reduce(IncrementCounterButtonClickedAction action, ExampleState currentState) => currentState with
    {
        Counter = currentState.Counter + 1
    };
}