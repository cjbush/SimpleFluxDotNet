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

    private sealed record IncrementCounterAction : IFluxAction
    {
    }

    private sealed class IncrementCounterReducer : AbstractFluxReducer<TestState, IncrementCounterAction>
    {
        public override TestState Reduce(IncrementCounterAction @event, TestState currentState) =>
            currentState with { Counter = currentState.Counter + 1 };
    }

    private sealed class ResetCounterReducer : AbstractFluxReducer<TestState, IncrementCounterAction>
    {
        public override TestState Reduce(IncrementCounterAction @event, TestState currentState)
        {
            if (currentState.Counter > 1)
            {
                return currentState with { CounterOverflowed = true };
            }
            return currentState;
        }
    }

    private sealed record NameChangedAction : IFluxAction
    {
        public required string NewName { get; init; }
    }

    private sealed class NameChangedActionCreator : IFluxActionCreator<NameChangedAction>
    {
        public async Task<NameChangedAction> CreateAsync(CancellationToken ct = default)
        {
            await Task.Delay(500, ct); // simulate API call or something
            return new() { NewName = "Joe" };
        }
    }

    private sealed class NameChangedReducer : AbstractFluxReducer<AnotherTestState, NameChangedAction>
    {
        public override AnotherTestState Reduce(NameChangedAction @event, AnotherTestState currentState) =>
            currentState with { Name = @event.NewName };
    }


    [Test]
    public static async Task Flux_ShouldTriggerEventFromActionAndHandleInReducer()
    {
        var stateHasChanged = false;
        using var sp = new ServiceCollection().AddFluxStateManagement(flux =>
        {
            flux.ForState(new AnotherTestState { Name = "Test" })
                    .ForAction<NameChangedAction>()
                        .WithCreator<NameChangedActionCreator>()
                        .UseReducer<NameChangedReducer>()
                .ForState<TestState>()
                    .ForAction<IncrementCounterAction>()
                        .UseReducer<IncrementCounterReducer>()
                        .UseReducer<ResetCounterReducer>();
        }).BuildServiceProvider();
        var stateStore = sp.GetRequiredService<IFluxStateStore<TestState>>();
        stateStore.OnStateChanged += (sender, args, ct) =>
        {
            stateHasChanged = true;
            return Task.CompletedTask;
        };
        var dispatcher = sp.GetRequiredService<IFluxDispatcher>();
        var oldState = stateStore.Current with { };

        await dispatcher.PublishAsync<IncrementCounterAction>(CancellationToken.None);

        oldState.Counter.Should().Be(0);
        stateStore.Current.Counter.Should().Be(1);
        stateStore.Current.CounterOverflowed.Should().BeFalse();
        stateHasChanged.Should().BeTrue();

        await dispatcher.PublishAsync<IncrementCounterAction>(CancellationToken.None);
        stateStore.Current.Counter.Should().Be(2);
        stateStore.Current.CounterOverflowed.Should().BeTrue();
        stateHasChanged.Should().BeTrue();

        var nameAction = sp.GetRequiredService<IFluxActionCreator<NameChangedAction>>();
        var nameState = sp.GetRequiredService<IFluxStateStore<AnotherTestState>>();

        await dispatcher.PublishAsync(await nameAction.CreateAsync(), CancellationToken.None);

        nameState.Current.Name.Should().Be("Joe");
    }

}
