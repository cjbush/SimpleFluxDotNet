using SimpleFluxDotNet.Abstractions;

namespace SimpleFluxDotNet;


internal sealed class DelegateFluxReducer<TState, TAction> : IFluxReducer<TState, TAction> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    private readonly ReducerDelegate<TState, TAction> _reducer;

    public DelegateFluxReducer(ReducerDelegate<TState, TAction> reducer)
    {
        _reducer = reducer;
    }

    public Type ActionType => typeof(TAction);

    public TState Reduce(object action, TState currentState) =>
        _reducer((TAction)action, currentState);

    public TState Reduce(TAction action, TState currentState) =>
        _reducer(action, currentState);
}
