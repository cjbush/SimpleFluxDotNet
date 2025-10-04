using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleFluxDotNet.Abstractions;
using SimpleFluxDotNet.Extensions;

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


    private static class IncrementCounterReducer
    {
        public static TestState Reduce(TestState currentState) =>
            currentState with { Counter = currentState.Counter + 1 };
    }

    private static class ResetIncrementCounterReducer
    {
        public static TestState Reduce(TestState currentState)
        {
            if (currentState.Counter >= 5)
            {
                return currentState with { Counter = 0 };
            }
            return currentState;
        }
    }

    private static class DecrementCounterReducer
    {
        public static TestState Reduce(TestState currentState) =>
            currentState with { Counter = currentState.Counter - 1 };
    }

    private sealed class ResetDecrementCounterReducer : IFluxReducer<TestState, DecrementCounterAction>
    {
        public TestState Reduce(DecrementCounterAction action, TestState currentState)
        {
            if (currentState.Counter < 0)
            {
                return currentState with { Counter = 0 };
            }
            return currentState;
        }
    }


    private sealed class NameChangedMiddleware : IFluxMiddleware<AnotherTestState, NameChangedAction>
    {
        public async Task DispatchAsync(NameChangedAction action, FluxMiddlewareContext<AnotherTestState> context, CancellationToken ct = default)
        {
            await Task.Delay(500, ct); // simulate API call or something
            await context.Next(new NameChangedAction("Joe"));
        }
    }

    private static class NameChangedReducer
    {
        public static AnotherTestState Reduce(NameChangedAction @event, AnotherTestState currentState) =>
            currentState with { Name = @event.NewName };
    }


    [Test]
    public static async Task Flux_ShouldTriggerEventFromActionAndHandleInReducer()
    {
        var stateHasChanged = false;
        using var sp = new ServiceCollection().AddLogging(logging => logging.AddConsole()).AddFluxStateManagement(flux =>
        {
            flux.ConfigureStateContainer(new AnotherTestState { Name = "Test" }, state =>
            {
                state.HandleAction<NameChangedAction>(action =>
                {
                    action.UseMiddleware<NameChangedMiddleware>()
                          .UseReducer(NameChangedReducer.Reduce);
                });
            }).ConfigureStateContainer<TestState>(state =>
            {
                state.HandleAction<IncrementCounterAction>(action =>
                {
                    action.UseReducer(IncrementCounterReducer.Reduce)
                          .UseReducer(ResetIncrementCounterReducer.Reduce);
                });
                state.HandleAction<DecrementCounterAction>(action =>
                {
                    action.UseReducer(DecrementCounterReducer.Reduce)
                          .UseReducer<ResetDecrementCounterReducer>();
                });
            });
        }).BuildServiceProvider();
        var testState = sp.GetRequiredService<IFluxStateStore<TestState>>();
        testState.OnStateChanged += (sender, args, ct) =>
        {
            stateHasChanged = true;
            return Task.CompletedTask;
        };
        var oldState = testState.Current with { };

        await testState.DispatchAsync<IncrementCounterAction>(CancellationToken.None);

        oldState.Counter.Should().Be(0);
        testState.Current.Counter.Should().Be(1);
        stateHasChanged.Should().BeTrue();

        static IncrementCounterAction Increment() => new();
        static DecrementCounterAction Decrement() => new();

        await testState.DispatchAsync(Increment());
        await testState.DispatchAsync(Decrement());
        await testState.DispatchAsync(Decrement());
        await testState.DispatchAsync(Increment());
        await testState.DispatchAsync(Increment());
        await testState.DispatchAsync(Increment());
        await testState.DispatchAsync(Increment());
        await testState.DispatchAsync(Increment());

        testState.Current.Counter.Should().Be(0);
        stateHasChanged.Should().BeTrue();


        var anotherState = sp.GetRequiredService<IFluxStateStore<AnotherTestState>>();

        await anotherState.DispatchAsync(new NameChangedAction("Chris"));

        anotherState.Current.Name.Should().Be("Joe");
    }

}
