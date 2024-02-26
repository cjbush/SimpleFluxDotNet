namespace SimpleFluxDotNet;

public delegate Task AsyncEventHandler(object sender, EventArgs args, CancellationToken ct = default);

public interface IFluxStateStore<out TState> where TState : AbstractFluxState
{
    TState Current { get; }

    event AsyncEventHandler OnStateChanged;
}

internal delegate TState InitialStateProvider<out TState>() where TState : AbstractFluxState;

internal sealed class GenericFluxStateStore<TState> : IFluxStateStore<TState> where TState : AbstractFluxState
{
    public event AsyncEventHandler OnStateChanged;

    public TState Current { get; private set; }

    public GenericFluxStateStore(InitialStateProvider<TState> initialState, IFluxDispatcher dispatcher, IEnumerable<IFluxReducer<TState>> reducers)
    {
        OnStateChanged = (sender, args, ct) => Task.CompletedTask;
        Current = initialState();
        foreach (var reducer in reducers)
        {
            reducer.OnStateChanged += async (newState, ct) =>
            {
                Current = newState;
                await OnStateChanged(this, EventArgs.Empty, ct);
            };
            dispatcher.Subscribe(reducer.EventType, async (@event, ct) =>
            {
                await reducer.ReduceAsync(@event, Current, ct);
            });
        }
    }

}
