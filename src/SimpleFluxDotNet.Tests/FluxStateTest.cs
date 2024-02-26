using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleFluxDotNet.Tests;

[TestFixture, Parallelizable(ParallelScope.All)]
internal static class FluxStateTest
{


    private sealed record TestState : AbstractFluxState
    {
        public int Counter { get; init; }
        public bool CounterOverflowed { get; init; }
    }

    public sealed record AnotherTestState : AbstractFluxState
    {
        public required string Name { get; init; }
    }

    private sealed record IncrementCounterEvent : IFluxEvent
    {
    }

    private sealed class IncrementCounterReducer : AbstractFluxReducer<TestState, IncrementCounterEvent>
    {
        public override Task ReduceAsync(IncrementCounterEvent @event, TestState currentState, CancellationToken ct = default) =>
            StateHasChanged(currentState with { Counter = currentState.Counter + 1 }, ct);
    }

    private sealed class ResetCounterReducer : AbstractFluxReducer<TestState, IncrementCounterEvent>
    {
        public override async Task ReduceAsync(IncrementCounterEvent @event, TestState currentState, CancellationToken ct = default)
        {
            if (currentState.Counter > 1)
            {
                await StateHasChanged(currentState with { CounterOverflowed = true }, ct);
            }
        }
    }

    private sealed record NameChangedEvent : IFluxEvent
    {
        public required string NewName { get; init; }
    }

    private sealed class NameChangedReducer : AbstractFluxReducer<AnotherTestState, NameChangedEvent>
    {
        public override Task ReduceAsync(NameChangedEvent @event, AnotherTestState currentState, CancellationToken ct = default) =>
            StateHasChanged(currentState with { Name = @event.NewName }, ct);
    }


    [Test]
    public static async Task Flux_ShouldTriggerEventFromActionAndHandleInReducer()
    {
        var stateHasChanged = false;
        using var sp = new ServiceCollection().AddFluxStateManagement(flux =>
        {
            flux.ForState(new AnotherTestState { Name = "Test" })
                    .HandleEvent<NameChangedEvent>()
                        .With<NameChangedReducer>()
                .ForState<TestState>()
                    .HandleEvent<IncrementCounterEvent>()
                        .With<IncrementCounterReducer>()
                        .With<ResetCounterReducer>();
        }).BuildServiceProvider();
        var stateStore = sp.GetRequiredService<IFluxStateStore<TestState>>();
        stateStore.OnStateChanged += (sender, args, ct) =>
        {
            stateHasChanged = true;
            return Task.CompletedTask;
        };
        var action = sp.GetRequiredService<IFluxAction<IncrementCounterEvent>>();
        var oldState = stateStore.Current with { };

        await action.DispatchAsync(CancellationToken.None);

        oldState.Counter.Should().Be(0);
        stateStore.Current.Counter.Should().Be(1);
        stateStore.Current.CounterOverflowed.Should().BeFalse();
        stateHasChanged.Should().BeTrue();

        await action.DispatchAsync(CancellationToken.None);
        stateStore.Current.Counter.Should().Be(2);
        stateStore.Current.CounterOverflowed.Should().BeTrue();
        stateHasChanged.Should().BeTrue();

        var nameAction = sp.GetRequiredService<IFluxAction<NameChangedEvent>>();
        var nameState = sp.GetRequiredService<IFluxStateStore<AnotherTestState>>();

        await nameAction.DispatchAsync(new() { NewName = "Joe" });

        nameState.Current.Name.Should().Be("Joe");
    }

}
