namespace SimpleFluxDotNet.Abstractions;

public delegate Task AsyncEventHandler(object sender, EventArgs args, CancellationToken ct = default);

public interface IFluxStateStore<out TState> where TState : AbstractFluxState
{
    TState Current { get; }

    event AsyncEventHandler OnStateChanged;

    Task DispatchAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction;
    Task DispatchAsync<TAction>(CancellationToken ct = default) where TAction : class, IFluxAction, new();
}

internal delegate TState InitialStateProvider<out TState>() where TState : AbstractFluxState;
