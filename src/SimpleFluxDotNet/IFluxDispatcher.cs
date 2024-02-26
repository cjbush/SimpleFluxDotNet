namespace SimpleFluxDotNet;


internal delegate Task AsyncFluxEventCallback(IFluxEvent @event, CancellationToken ct);

internal interface IFluxDispatcher
{
    void Subscribe(Type eventType, AsyncFluxEventCallback callback);

    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class, IFluxEvent;
}

internal sealed class FluxDispatcher : IFluxDispatcher
{

    private readonly List<KeyValuePair<Type, object>> _callbacks = [];

    public void Subscribe(Type eventType, AsyncFluxEventCallback callback) => _callbacks.Add(new(eventType, callback));

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class, IFluxEvent
    {
        var eventType = typeof(TEvent);
        var callbacks = _callbacks.ToLookup(_ => _.Key);
        foreach (var callback in callbacks[eventType].Select(_ => _.Value))
        {
            if (callback is AsyncFluxEventCallback asyncCallback)
            {
                await Task.Run(() => asyncCallback(@event, ct), ct);
            }
        }
    }

}
