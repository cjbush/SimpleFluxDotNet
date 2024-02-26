namespace SimpleFluxDotNet;


public delegate Task AsyncFluxActionCallback(IFluxAction action, CancellationToken ct);

public interface IFluxDispatcher
{
    void Subscribe(Type actionType, AsyncFluxActionCallback callback);

    Task DispatchAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction;
}

internal interface IInternalFluxDispatcher : IFluxDispatcher
{
    Task DispatchAsync(Type actionType, IFluxAction action, CancellationToken ct = default);
}

public static class FluxDispatcherExtensions
{
    public static Task DispatchAsync<TAction>(this IFluxDispatcher dispatcher, CancellationToken ct = default) where TAction : class, IFluxAction, new() =>
        dispatcher.DispatchAsync<TAction>(new(), ct);

    public static async Task DispatchAsync(this IFluxDispatcher dispatcher, List<IFluxAction> actions, CancellationToken ct = default)
    {
        if (dispatcher is not IInternalFluxDispatcher internalDispatcher)
        {
            throw new InvalidOperationException($@"This operation is only available when using the stock dispatcher");
        }
        foreach (var action in actions)
        {
            await internalDispatcher.DispatchAsync(action.GetType(), action, ct);
        }
    }

}

internal sealed class FluxDispatcher : IInternalFluxDispatcher
{

    private readonly List<KeyValuePair<Type, object>> _callbacks = [];

    public void Subscribe(Type actionType, AsyncFluxActionCallback callback) => _callbacks.Add(new(actionType, callback));

    public Task DispatchAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction =>
        DispatchAsync(typeof(TAction), action, ct);

    public async Task DispatchAsync(Type actionType, IFluxAction action, CancellationToken ct = default)
    {
        var callbacks = _callbacks.ToLookup(_ => _.Key);
        foreach (var callback in callbacks[actionType].Select(_ => _.Value))
        {
            if (callback is AsyncFluxActionCallback asyncCallback)
            {
                await Task.Run(() => asyncCallback(action, ct), ct);
            }
        }
    }
}
