using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleFluxDotNet.Tests;

[TestFixture, Parallelizable(ParallelScope.All)]
internal static class FluxStateTest
{


    private sealed record TestState : AbstractFluxState
    {
        public int Counter { get; init; }
    }

    public sealed record AnotherTestState : AbstractFluxState
    {
        public required string Name { get; init; }
    }

    private sealed record IncrementCounterAction : IFluxAction { }
    private sealed record DecrementCounterAction : IFluxAction { }
    private sealed record NameChangedAction(string NewName) : IFluxAction { }


    private sealed class IncrementCounterReducer : AbstractFluxReducer<TestState, IncrementCounterAction>
    {
        public override TestState Reduce(IncrementCounterAction action, TestState currentState) =>
            currentState with { Counter = currentState.Counter + 1 };
    }

    private sealed class ResetIncrementCounterReducer : AbstractFluxReducer<TestState, IncrementCounterAction>
    {
        public override TestState Reduce(IncrementCounterAction action, TestState currentState)
        {
            if (currentState.Counter >= 5)
            {
                return currentState with { Counter = 0 };
            }
            return currentState;
        }
    }

    private sealed class DecrementCounterReducer : AbstractFluxReducer<TestState, DecrementCounterAction>
    {
        public override TestState Reduce(DecrementCounterAction action, TestState currentState) =>
            currentState with { Counter = currentState.Counter - 1 };
    }

    private sealed class ResetDecrementCounterReducer : AbstractFluxReducer<TestState, DecrementCounterAction>
    {
        public override TestState Reduce(DecrementCounterAction action, TestState currentState)
        {
            if (currentState.Counter < 0)
            {
                return currentState with { Counter = 0 };
            }
            return currentState;
        }
    }


    private sealed class NameChangedActionCreator : IFluxActionCreator<NameChangedAction>
    {
        public async Task<NameChangedAction> CreateAsync(CancellationToken ct = default)
        {
            await Task.Delay(500, ct); // simulate API call or something
            return new("Joe");
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
                        .UseReducer<ResetIncrementCounterReducer>()
                    .ForAction<DecrementCounterAction>()
                        .UseReducer<DecrementCounterReducer>()
                        .UseReducer<ResetDecrementCounterReducer>();
        }).BuildServiceProvider();
        var stateStore = sp.GetRequiredService<IFluxStateStore<TestState>>();
        stateStore.OnStateChanged += (sender, args, ct) =>
        {
            stateHasChanged = true;
            return Task.CompletedTask;
        };
        var dispatcher = sp.GetRequiredService<IFluxDispatcher>();
        var chainer = sp.GetRequiredService<IFluxActionChainer>();
        var oldState = stateStore.Current with { };

        await dispatcher.DispatchAsync<IncrementCounterAction>(CancellationToken.None);

        oldState.Counter.Should().Be(0);
        stateStore.Current.Counter.Should().Be(1);
        stateHasChanged.Should().BeTrue();

        static IncrementCounterAction Increment() => new();
        static DecrementCounterAction Decrement() => new();

        await chainer.Dispatch(Increment())
                     .Then(Decrement())
                     .Then(Decrement())
                     .Then(Increment())
                     .Then(Increment())
                     .Then(Increment())
                     .Then(Increment())
                     .Then(Increment())
                     .ExecuteAsync();
        stateStore.Current.Counter.Should().Be(0);
        stateHasChanged.Should().BeTrue();

        var nameAction = sp.GetRequiredService<IFluxActionCreator<NameChangedAction>>();
        var nameState = sp.GetRequiredService<IFluxStateStore<AnotherTestState>>();

        var nameChainer = sp.GetRequiredService<IFluxActionChainer>();
        await nameChainer.Dispatch(nameAction).ExecuteAsync();

        nameState.Current.Name.Should().Be("Joe");
    }

}
