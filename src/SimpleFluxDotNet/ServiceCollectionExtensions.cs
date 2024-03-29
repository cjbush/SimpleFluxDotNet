﻿using Microsoft.Extensions.DependencyInjection;

namespace SimpleFluxDotNet;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddFluxStateManagement(this IServiceCollection services, Action<FluxStateBuilder> builder)
    {
        services.AddSingleton<IFluxDispatcher, FluxDispatcher>();
        services.AddTransient<IFluxActionChainer, FluxActionChainer>();
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

    public FluxActionBuilder<TState> ForState<TState>() where TState : AbstractFluxState, new() => ForState<TState>(new());

    public FluxActionBuilder<TState> ForState<TState>(TState initialState) where TState : AbstractFluxState
    {
        _services.AddSingleton<InitialStateProvider<TState>>(() => initialState);
        _services.AddSingleton<IFluxStateStore<TState>, GenericFluxStateStore<TState>>();
        return new FluxActionBuilder<TState>(_services, this);
    }

}

public sealed class FluxActionBuilder<TState> where TState : AbstractFluxState
{
    private readonly IServiceCollection _services;
    private readonly FluxStateBuilder _parent;

    public FluxActionBuilder(IServiceCollection services, FluxStateBuilder parent)
    {
        _services = services;
        _parent = parent;
    }

    public FluxActionCreatorBuilder<TState, TAction> ForAction<TAction>() where TAction : class, IFluxAction
    {
        return new FluxActionCreatorBuilder<TState, TAction>(_services, this);
    }

    internal FluxStateBuilder Parent => _parent;
}

public sealed class FluxActionCreatorBuilder<TState, TAction> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    private readonly IServiceCollection _services;
    private readonly FluxActionBuilder<TState> _parent;

    public FluxActionCreatorBuilder(IServiceCollection services, FluxActionBuilder<TState> parent)
    {
        _services = services;
        _parent = parent;
    }

    public FluxReducerBuilder<TState, TAction> WithCreator<TActionCreator>() where TActionCreator : class, IFluxActionCreator<TAction>
    {
        _services.AddSingleton<IFluxActionCreator<TAction>, TActionCreator>();
        return new FluxReducerBuilder<TState, TAction>(_services, _parent);
    }

    public FluxReducerBuilder<TState, TAction> UseReducer<TReducer>() where TReducer : class, IFluxReducer<TState, TAction>
    {
        return new FluxReducerBuilder<TState, TAction>(_services, _parent).UseReducer<TReducer>();
    }

}

public sealed class FluxReducerBuilder<TState, TAction> where TState : AbstractFluxState where TAction : class, IFluxAction
{
    private readonly IServiceCollection _services;
    private readonly FluxActionBuilder<TState> _parent;

    public FluxReducerBuilder(IServiceCollection services, FluxActionBuilder<TState> parent)
    {
        _services = services;
        _parent = parent;
    }

    public FluxReducerBuilder<TState, TAction> UseReducer<TReducer>() where TReducer : class, IFluxReducer<TState, TAction>
    {
        _services.AddSingleton<IFluxReducer<TState>, TReducer>();
        return this;
    }

    public FluxActionCreatorBuilder<TState, TNewAction> ForAction<TNewAction>() where TNewAction : class, IFluxAction
    {
        return _parent.ForAction<TNewAction>();
    }

    public FluxActionBuilder<TNewState> ForState<TNewState>() where TNewState : AbstractFluxState, new() =>
        ForState<TNewState>(new());

    public FluxActionBuilder<TNewState> ForState<TNewState>(TNewState initialState) where TNewState : AbstractFluxState
    {
        return _parent.Parent.ForState(initialState);
    }
}
