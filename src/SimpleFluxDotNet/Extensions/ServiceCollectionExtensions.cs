using Microsoft.Extensions.DependencyInjection;
using SimpleFluxDotNet.Abstractions;

namespace SimpleFluxDotNet.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddFluxStateManagement(this IServiceCollection services, Action<FluxStateBuilder> builder)
    {
        services.AddSingleton<IFluxDispatcherQueue, FluxDispatcherQueue>();
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

    public FluxStateBuilder ConfigureStateContainer<TState>(Action<FluxStateBuilder<TState>> builder) where TState : AbstractFluxState, new() =>
        ConfigureStateContainer(new TState(), builder);

    public FluxStateBuilder ConfigureStateContainer<TState>(TState initialState, Action<FluxStateBuilder<TState>> builder) where TState : AbstractFluxState
    {
        _services.AddSingleton<InitialStateProvider<TState>>(() => initialState);
        _services.AddSingleton<IFluxStateStore<TState>, GenericFluxStateStore<TState>>();
        var stateBuilder = new FluxStateBuilder<TState>(_services);
        builder(stateBuilder);
        return this;
    }

}

public sealed class FluxStateBuilder<TState> where TState : AbstractFluxState
{
    private readonly IServiceCollection _services;
    public FluxStateBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public FluxStateBuilder<TState> HandleAction<TAction>(Action<FluxActionBuilder<TState, TAction>> builder) where TAction : class, IFluxAction
    {
        var actionBuilder = new FluxActionBuilder<TState, TAction>(_services);
        builder(actionBuilder);
        return this;
    }

}

public sealed class FluxActionBuilder<TState, TAction> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    private readonly IServiceCollection _services;
    public FluxActionBuilder(IServiceCollection services)
    {
        _services = services;
    }
    public FluxActionBuilder<TState, TAction> UseMiddleware<TMiddleware>() where TMiddleware : class, IFluxMiddleware<TState, TAction>
    {
        _services.AddScoped<IFluxMiddleware<TState>, TMiddleware>();
        _services.AddScoped<IFluxMiddleware<TState, TAction>, TMiddleware>();
        return this;
    }

    public FluxActionBuilder<TState, TAction> UseReducer<TReducer>() where TReducer : class, IFluxReducer<TState, TAction>
    {
        _services.AddSingleton<IFluxReducer<TState>, TReducer>();
        return this;
    }

    public FluxActionBuilder<TState, TAction> UseReducer(ReducerDelegate<TState> reducer)
    {
        var reducerWrapper = new ReducerDelegate<TState, TAction>((action, state) => reducer(state));
        _services.AddSingleton<IFluxReducer<TState>>(sp => new DelegateFluxReducer<TState, TAction>(reducerWrapper));
        return this;
    }
    public FluxActionBuilder<TState, TAction> UseReducer(ReducerDelegate<TState, TAction> reducer)
    {
        _services.AddSingleton<IFluxReducer<TState>>(sp => new DelegateFluxReducer<TState, TAction>(reducer));
        return this;
    }
}