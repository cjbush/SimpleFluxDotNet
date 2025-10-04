# SimpleFluxDotNet

A simple Flux architecture implementation for .NET applications with minimal dependencies and boilerplate code. Particularly useful for Blazor applications.

[![NuGet](https://img.shields.io/nuget/v/SimpleFlux.NET.svg)](https://www.nuget.org/packages/SimpleFlux.NET/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- 🪶 Lightweight Flux implementation
- ⚡ First-class Blazor support
- 📦 Minimal dependencies
- 🔒 Type-safe state management
- 💉 Easy integration with dependency injection
- 🔌 Built-in middleware support

## Installation

Install via NuGet:

```bash
dotnet add package SimpleFlux.NET
```

## Quick Start

1. Define your state:

```csharp
public sealed record ExampleState : AbstractFluxState
{
    public int Counter { get; init; }
    public WeatherForecast[]? Forecasts { get; init; }
}
```

2. Create actions:

```csharp
public sealed record IncrementCounterButtonClickedAction : IFluxAction { }
```

3. Create a reducer:

```csharp
public sealed class IncrementCounterReducer : IFluxReducer<ExampleState, IncrementCounterButtonClickedAction>
{
    public ExampleState Reduce(IncrementCounterButtonClickedAction action, ExampleState currentState) => currentState with
    {
        Counter = currentState.Counter + 1
    };
}
```

4. Set up in your Blazor application:

```csharp
builder.Services.AddFluxStateManagement(flux =>
{
    flux.ConfigureStateContainer<ExampleState>(state =>
    {
        state.HandleAction<IncrementCounterButtonClickedAction>(action => 
            action.UseReducer<IncrementCounterReducer>());
        
        // You can also define inline reducers
        state.HandleAction<SomeOtherAction>(action =>
        {
            action.UseReducer(static (action, state) => state with
            {
                // Update state here
            });
        });
        
        // Or use middleware for side effects
        state.HandleAction<LoadDataAction>(action => 
            action.UseMiddleware<LoadDataMiddleware>());
    });
});
```

5. Use in components:

```csharp
@page "/counter"
@using SimpleFluxDotNet.Abstractions
@inherits FluxComponent<ExampleState>

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @State.Current.Counter</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    private Task IncrementCount() =>
        State.DispatchAsync(new IncrementCounterButtonClickedAction());
}
```

## Project Structure

- `SimpleFluxDotNet` - Core library with Flux implementation
- `SimpleFluxDotNet.Example` - Example Blazor application showcasing usage
- `SimpleFluxDotNet.Tests` - Unit tests

## Documentation

### Core Concepts

- **State**: Immutable state container that holds your application's data
- **Actions**: Plain objects describing what happened
- **Reducers**: Pure functions that specify how the state changes in response to actions
- **Store**: Holds the state and handles dispatching of actions
- **Middleware**: Intercepts actions for side effects, logging, etc.

### Components

#### IFluxAction
The base interface for all actions in your application.

#### AbstractFluxState
Base class for your application's state objects.

#### FluxComponent
Base component class that provides access to state and dispatch capabilities.

#### IFluxReducer<TState, TAction>
Interface for implementing reducers that handle specific action types. Reducers are pure functions that take the current state and an action, and return a new state.

```csharp
public interface IFluxReducer<TState, TAction> where TState : AbstractFluxState where TAction : IFluxAction
{
    TState Reduce(TAction action, TState currentState);
}
```

#### IFluxMiddleware<TState, TAction>
Interface for implementing middleware that handles side effects for specific action types. Middleware can perform async operations and dispatch additional actions.

```csharp
public interface IFluxMiddleware<TState, TAction> where TState : AbstractFluxState where TAction : IFluxAction
{
    Task HandleAsync(TAction action, TState currentState, IFluxDispatcher dispatcher);
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

[cjbush](https://github.com/cjbush)
