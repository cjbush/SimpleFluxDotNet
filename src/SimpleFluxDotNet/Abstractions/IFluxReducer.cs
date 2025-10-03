namespace SimpleFluxDotNet.Abstractions;


public delegate TState ReducerDelegate<TState>(TState currentState);
public delegate TState ReducerDelegate<TState, in TAction>(TAction action, TState currentState);

public interface IFluxReducer<TState> where TState : AbstractFluxState
{
    Type ActionType { get; }
    TState Reduce(object action, TState currentState);
}

public interface IFluxReducer<TState, in TAction> : IFluxReducer<TState> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    Type IFluxReducer<TState>.ActionType => typeof(TAction);
    TState IFluxReducer<TState>.Reduce(object action, TState currentState) => Reduce((TAction)action, currentState);
    TState Reduce(TAction action, TState currentState);
}
