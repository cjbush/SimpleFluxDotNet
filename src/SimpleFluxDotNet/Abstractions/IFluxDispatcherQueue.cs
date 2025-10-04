namespace SimpleFluxDotNet.Abstractions;


internal delegate Task AsyncFluxActionCallback(IFluxAction action, CancellationToken ct);

internal interface IFluxDispatcherQueue
{
    void Subscribe(Type actionType, AsyncFluxActionCallback callback);
    Task DispatchAsync(Type actionType, IFluxAction action, CancellationToken ct = default);

    Task DispatchAsync<TAction>(TAction action, CancellationToken ct = default) where TAction : class, IFluxAction;
}

