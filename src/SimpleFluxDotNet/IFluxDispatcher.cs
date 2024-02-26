namespace SimpleFluxDotNet;


public delegate Task AsyncFluxActionCallback(IFluxAction action, CancellationToken ct);

public interface IFluxDispatcher
{
    void Subscribe(Type actionType, AsyncFluxActionCallback callback);

    Task PublishAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction;
}

public static class FluxDispatcherExtensions
{
    public static Task PublishAsync<TAction>(this IFluxDispatcher dispatcher, CancellationToken ct = default) where TAction : class, IFluxAction, new() =>
        dispatcher.PublishAsync<TAction>(new(), ct);
}

internal sealed class FluxDispatcher : IFluxDispatcher
{

    private readonly List<KeyValuePair<Type, object>> _callbacks = [];

    public void Subscribe(Type actionType, AsyncFluxActionCallback callback) => _callbacks.Add(new(actionType, callback));

    public async Task PublishAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction
    {
        var eventType = typeof(TAction);
        var callbacks = _callbacks.ToLookup(_ => _.Key);
        foreach (var callback in callbacks[eventType].Select(_ => _.Value))
        {
            if (callback is AsyncFluxActionCallback asyncCallback)
            {
                await Task.Run(() => asyncCallback(action, ct), ct);
            }
        }
    }

}
