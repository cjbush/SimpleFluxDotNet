namespace SimpleFluxDotNet;


public delegate Task AsyncFluxActionCallback(IFluxAction action, CancellationToken ct);

public interface IFluxDispatcher
{
    void Subscribe(Type actionType, AsyncFluxActionCallback callback);
    Task DispatchAsync(Type actionType, IFluxAction action, CancellationToken ct = default);

    Task DispatchAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction;
}

public static class FluxDispatcherExtensions
{
    public static Task DispatchAsync<TAction>(this IFluxDispatcher dispatcher, CancellationToken ct = default) where TAction : class, IFluxAction, new() =>
        dispatcher.DispatchAsync<TAction>(new(), ct);

}

internal sealed class FluxDispatcher : IFluxDispatcher
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
