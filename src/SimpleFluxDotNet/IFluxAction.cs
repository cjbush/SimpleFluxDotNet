namespace SimpleFluxDotNet;

public interface IFluxAction<in TEvent> where TEvent : class, IFluxEvent
{
    Task DispatchAsync(TEvent @event, CancellationToken ct = default);
}

public static class FluxActionExtensions
{
    public static Task DispatchAsync<TEvent>(this IFluxAction<TEvent> action, CancellationToken ct = default) where TEvent : class, IFluxEvent, new() =>
        action.DispatchAsync(new TEvent(), ct);
}

internal sealed class GenericFluxAction<TEvent> : IFluxAction<TEvent> where TEvent : class, IFluxEvent
{
    private readonly IFluxDispatcher _dispatcher;

    public GenericFluxAction(IFluxDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task DispatchAsync(TEvent @event, CancellationToken ct = default)
    {
        await _dispatcher.PublishAsync(@event, ct);
    }
}
