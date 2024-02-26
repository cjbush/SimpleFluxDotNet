using SimpleFluxDotNet.Example.Events;
using SimpleFluxDotNet.Example.States;

namespace SimpleFluxDotNet.Example.Reducers;

public sealed class IncrementCounterReducer : AbstractFluxReducer<ExampleState, IncrementCounterButtonClickedAction>
{
    public override ExampleState Reduce(IncrementCounterButtonClickedAction @event, ExampleState currentState) =>
        currentState with { Counter = currentState.Counter + 1 };
}
