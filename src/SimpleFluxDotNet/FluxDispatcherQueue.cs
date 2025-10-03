using SimpleFluxDotNet.Abstractions;

namespace SimpleFluxDotNet;

internal sealed class FluxDispatcherQueue : IFluxDispatcherQueue
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
