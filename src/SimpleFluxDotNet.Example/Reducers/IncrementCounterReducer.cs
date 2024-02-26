using SimpleFluxDotNet.Example.Events;
using SimpleFluxDotNet.Example.States;

namespace SimpleFluxDotNet.Example.Reducers;

public sealed class IncrementCounterReducer : AbstractFluxReducer<ExampleState, IncrementCounterButtonClickedEvent>
{
    public override ExampleState Reduce(IncrementCounterButtonClickedEvent @event, ExampleState currentState) =>
        currentState with { Counter = currentState.Counter + 1 };
}
