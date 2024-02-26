namespace SimpleFluxDotNet;

public interface IFluxActionChainer
{
    FluxActionChainBuilder Dispatch<TAction>(TAction action) where TAction : class, IFluxAction;
    FluxActionChainBuilder Dispatch<TAction>() where TAction : class, IFluxAction, new();
    FluxActionChainBuilder Dispatch<TAction, TActionCreator>(TActionCreator creator)
        where TAction : class, IFluxAction
        where TActionCreator : class, IFluxActionCreator<TAction>;
}

internal sealed class FluxActionChainer : IFluxActionChainer
{
    private readonly IFluxDispatcher _dispatcher;
    private readonly List<Func<CancellationToken, Task<IFluxAction>>> _actionCreators = [];

    public FluxActionChainer(IFluxDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public FluxActionChainBuilder Dispatch<TAction>(TAction action) where TAction : class, IFluxAction
    {
        _actionCreators.Add((ct) => Task.FromResult((IFluxAction)action));
        return new FluxActionChainBuilder(_dispatcher, _actionCreators);
    }

    public FluxActionChainBuilder Dispatch<TAction>() where TAction : class, IFluxAction, new() =>
        Dispatch(new TAction());

    public FluxActionChainBuilder Dispatch<TAction, TActionCreator>(TActionCreator creator) where TActionCreator : class, IFluxActionCreator<TAction> where TAction : class, IFluxAction
    {
        _actionCreators.Add(async (ct) => await creator.CreateAsync(ct));
        return new FluxActionChainBuilder(_dispatcher, _actionCreators);
    }

}


public sealed class FluxActionChainBuilder
{
    private readonly IFluxDispatcher _dispatcher;
    private readonly List<Func<CancellationToken, Task<IFluxAction>>> _actionCreators;

    internal FluxActionChainBuilder(IFluxDispatcher dispatcher, List<Func<CancellationToken, Task<IFluxAction>>> actionCreators)
    {
        _dispatcher = dispatcher;
        _actionCreators = actionCreators;
    }

    public FluxActionChainBuilder Then<TAction>(TAction action) where TAction : class, IFluxAction
    {
        _actionCreators.Add((ct) => Task.FromResult((IFluxAction)action));
        return this;
    }

    public FluxActionChainBuilder Then<TAction>() where TAction : class, IFluxAction, new() =>
        Then(new TAction());

    public FluxActionChainBuilder Then<TAction, TActionCreator>(TActionCreator creator) where TActionCreator : class, IFluxActionCreator<TAction> where TAction : class, IFluxAction
    {
        _actionCreators.Add(async (ct) => await creator.CreateAsync(ct));
        return this;
    }


    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        foreach (var actionCreator in _actionCreators)
        {
            var action = await actionCreator(ct);
            await _dispatcher.DispatchAsync(action.GetType(), action, ct);
        }
    }

}
