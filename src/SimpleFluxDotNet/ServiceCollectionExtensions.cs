using Microsoft.Extensions.DependencyInjection;

namespace SimpleFluxDotNet;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddFluxStateManagement(this IServiceCollection services, Action<FluxStateBuilder> builder)
    {
        services.AddSingleton<IFluxDispatcher, FluxDispatcher>();
        builder(new FluxStateBuilder(services));
        return services;
    }

}

public sealed class FluxStateBuilder
{
    private readonly IServiceCollection _services;

    public FluxStateBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public FluxEventBuilder<TState> ForState<TState>() where TState : AbstractFluxState, new() => ForState<TState>(new());

    public FluxEventBuilder<TState> ForState<TState>(TState initialState) where TState : AbstractFluxState
    {
        _services.AddSingleton<InitialStateProvider<TState>>(() => initialState);
        _services.AddSingleton<IFluxStateStore<TState>, GenericFluxStateStore<TState>>();
        return new FluxEventBuilder<TState>(_services, this);
    }

}

public sealed class FluxEventBuilder<TState> where TState : AbstractFluxState
{
    private readonly IServiceCollection _services;
    private readonly FluxStateBuilder _parent;

    public FluxEventBuilder(IServiceCollection services, FluxStateBuilder parent)
    {
        _services = services;
        _parent = parent;
    }

    public FluxReducerBuilder<TState, TEvent> HandleEvent<TEvent>() where TEvent : class, IFluxEvent
    {
        _services.AddSingleton<IFluxAction<TEvent>, GenericFluxAction<TEvent>>();
        return new FluxReducerBuilder<TState, TEvent>(_services, this);
    }

    internal FluxStateBuilder Parent => _parent;
}

public sealed class FluxReducerBuilder<TState, TEvent> where TState : AbstractFluxState where TEvent : class, IFluxEvent
{
    private readonly IServiceCollection _services;
    private readonly FluxEventBuilder<TState> _parent;

    public FluxReducerBuilder(IServiceCollection services, FluxEventBuilder<TState> parent)
    {
        _services = services;
        _parent = parent;
    }

    public FluxReducerBuilder<TState, TEvent> With<TReducer>() where TReducer : class, IFluxReducer<TState, TEvent>
    {
        _services.AddSingleton<IFluxReducer<TState>, TReducer>();
        return this;
    }

    public FluxReducerBuilder<TState, TNewEvent> HandleEvent<TNewEvent>() where TNewEvent : class, IFluxEvent
    {
        return _parent.HandleEvent<TNewEvent>();
    }

    public FluxEventBuilder<TNewState> ForState<TNewState>() where TNewState : AbstractFluxState, new() =>
        ForState<TNewState>(new());

    public FluxEventBuilder<TNewState> ForState<TNewState>(TNewState initialState) where TNewState : AbstractFluxState
    {
        return _parent.Parent.ForState(initialState);
    }
}
