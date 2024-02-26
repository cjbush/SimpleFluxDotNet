namespace SimpleFluxDotNet;


public delegate Task AsyncStateHasChangedHandler<in TState>(TState newState, CancellationToken ct = default) where TState : AbstractFluxState;

public interface IFluxReducer<TState> where TState : AbstractFluxState
{
    Type EventType { get; }
    Task ReduceAsync(object @event, TState currentState, CancellationToken ct = default);
    event AsyncStateHasChangedHandler<TState> OnStateChanged;
}

public interface IFluxReducer<TState, TEvent> : IFluxReducer<TState> where TState : AbstractFluxState where TEvent : class, IFluxEvent
{
    Task ReduceAsync(TEvent @event, TState currentState, CancellationToken ct = default);
}

public abstract class AbstractFluxReducer<TState, TEvent> : IFluxReducer<TState, TEvent> where TState : AbstractFluxState where TEvent : class, IFluxEvent
{
    protected AbstractFluxReducer()
    {
        OnStateChanged += (state, ct) => Task.CompletedTask;
    }

    public abstract Task ReduceAsync(TEvent @event, TState currentState, CancellationToken ct = default);

    public Type EventType => typeof(TEvent);

    public Task ReduceAsync(object @event, TState currentState, CancellationToken ct = default) =>
        ReduceAsync((TEvent)@event, currentState, ct);

    public event AsyncStateHasChangedHandler<TState> OnStateChanged;

    protected Task StateHasChanged(TState newState, CancellationToken ct = default) => OnStateChanged.Invoke(newState, ct);
}
