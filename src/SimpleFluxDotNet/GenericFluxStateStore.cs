using Microsoft.Extensions.DependencyInjection;
using SimpleFluxDotNet.Abstractions;

namespace SimpleFluxDotNet;

internal sealed class GenericFluxStateStore<TState> : IFluxStateStore<TState> where TState : AbstractFluxState
{
    private readonly IFluxDispatcherQueue _dispatcherQueue;
    private readonly IServiceScopeFactory _scopeFactory;

    public event AsyncEventHandler OnStateChanged;

    public TState Current { get; private set; }

    public GenericFluxStateStore(InitialStateProvider<TState> initialState, IFluxDispatcherQueue queue, IFluxDispatcherQueue dispatcherQueue, IServiceScopeFactory scopeFactory, IEnumerable<IFluxReducer<TState>> reducers)
    {
        OnStateChanged = (sender, args, ct) => Task.CompletedTask;
        Current = initialState();
        foreach (var reducer in reducers)
        {
            queue.Subscribe(reducer.ActionType, async (action, ct) =>
            {
                Current = reducer.Reduce(action, Current);
                await OnStateChanged(this, EventArgs.Empty, ct);
            });
        }

        _dispatcherQueue = dispatcherQueue;
        _scopeFactory = scopeFactory;
    }


    public async Task DispatchAsync<TAction>(CancellationToken ct = default) where TAction : class, IFluxAction, new() =>
        await DispatchAsync<TAction>(new(), ct);

    public async Task DispatchAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction =>
        await DispatchAsync((IFluxAction)action, ct);

    private async Task DispatchAsync(IFluxAction action, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var middlewares = scope.ServiceProvider.GetRequiredService<IEnumerable<IFluxMiddleware<TState>>>().Where(f => f.AppliesTo(typeof(TState), action.GetType()));
        FluxNextDelegate next = (x) => _dispatcherQueue.DispatchAsync(x.GetType(), x);
        foreach (var middleware in middlewares)
        {
            FluxNextDelegate localNext = next;
            next = (x) => middleware.DispatchAsync(action, new()
            {
                Next = localNext,
                Dispatch = (x) => DispatchAsync(x, ct),
                GetState = () => Current
            }, ct);
        }
        await next(action);
    }

}
