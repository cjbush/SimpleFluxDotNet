namespace SimpleFluxDotNet;


public interface IFluxReducer<TState> where TState : AbstractFluxState
{
    Type ActionType { get; }
    TState Reduce(object action, TState currentState);
}

public interface IFluxReducer<TState, TAction> : IFluxReducer<TState> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    TState Reduce(TAction action, TState currentState);
}

public abstract class AbstractFluxReducer<TState, TAction> : IFluxReducer<TState, TAction> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    public abstract TState Reduce(TAction action, TState currentState);

    public Type ActionType => typeof(TAction);

    public TState Reduce(object action, TState currentState) =>
        Reduce((TAction)action, currentState);
}
