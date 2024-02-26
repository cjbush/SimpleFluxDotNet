namespace SimpleFluxDotNet;


public interface IFluxReducer<TState> where TState : AbstractFluxState
{
    Type EventType { get; }
    TState Reduce(object @event, TState currentState);
}

public interface IFluxReducer<TState, TEvent> : IFluxReducer<TState> where TState : AbstractFluxState where TEvent : class, IFluxEvent
{
    TState Reduce(TEvent @event, TState currentState);
}

public abstract class AbstractFluxReducer<TState, TEvent> : IFluxReducer<TState, TEvent> where TState : AbstractFluxState where TEvent : class, IFluxEvent
{
    public abstract TState Reduce(TEvent @event, TState currentState);

    public Type EventType => typeof(TEvent);

    public TState Reduce(object @event, TState currentState) =>
        Reduce((TEvent)@event, currentState);
}
