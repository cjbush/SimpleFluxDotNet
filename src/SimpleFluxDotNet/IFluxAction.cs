namespace SimpleFluxDotNet;

public interface IFluxAction<in TEvent> where TEvent : class, IFluxEvent
{
    Task DispatchAsync(CancellationToken ct = default);
}

public abstract class AbstractFluxAction<TEvent> : IFluxAction<TEvent> where TEvent : class, IFluxEvent
{
    protected readonly IFluxDispatcher _dispatcher;

    protected AbstractFluxAction(IFluxDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task DispatchAsync(CancellationToken ct = default)
    {
        await _dispatcher.PublishAsync(await CreateEvent(), ct);
    }

    protected abstract Task<TEvent> CreateEvent();

}

internal sealed class GenericFluxAction<TEvent> : AbstractFluxAction<TEvent> where TEvent : class, IFluxEvent, new()
{

    public GenericFluxAction(IFluxDispatcher dispatcher) : base(dispatcher) { }

    protected override Task<TEvent> CreateEvent() => Task.FromResult(new TEvent());
}
