using SimpleFluxDotNet.Example.Events;
using SimpleFluxDotNet.Example.States;

namespace SimpleFluxDotNet.Example.Reducers;

public sealed class IncrementCounterReducer : AbstractFluxReducer<ExampleState, IncrementCounterButtonClickedEvent>
{
    public override Task ReduceAsync(IncrementCounterButtonClickedEvent @event, ExampleState currentState, CancellationToken ct = default) =>
        StateHasChanged(currentState with { Counter = currentState.Counter + 1 }, ct);
}
