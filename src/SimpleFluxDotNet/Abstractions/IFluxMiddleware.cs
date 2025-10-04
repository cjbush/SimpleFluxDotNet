namespace SimpleFluxDotNet.Abstractions;

public delegate Task FluxNextDelegate(IFluxAction action);
public delegate Task FluxDispatchDelegate(IFluxAction action);
public delegate TState FluxGetStateDelegate<out TState>() where TState : AbstractFluxState;

public interface IFluxMiddleware<TState> where TState : AbstractFluxState
{
    bool AppliesTo(Type state, Type action);
    Task DispatchAsync(IFluxAction action, FluxMiddlewareContext<TState> context, CancellationToken ct = default);
}

public interface IFluxMiddleware<TState, TAction> : IFluxMiddleware<TState> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    bool IFluxMiddleware<TState>.AppliesTo(Type state, Type action) => state == typeof(TState) && action == typeof(TAction);
    Task IFluxMiddleware<TState>.DispatchAsync(IFluxAction action, FluxMiddlewareContext<TState> context, CancellationToken ct) => DispatchAsync((TAction)action, context, ct);
    Task DispatchAsync(TAction action, FluxMiddlewareContext<TState> context, CancellationToken ct = default);
}

public sealed record FluxMiddlewareContext<TState> where TState : AbstractFluxState
{
    public required FluxNextDelegate Next { get; init; }
    public required FluxDispatchDelegate Dispatch { get; init; }
    public required FluxGetStateDelegate<TState> GetState { get; init; }
}
