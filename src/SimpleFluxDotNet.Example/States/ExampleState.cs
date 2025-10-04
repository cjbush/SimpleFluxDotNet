using SimpleFluxDotNet.Abstractions;

namespace SimpleFluxDotNet.Example.States;

public sealed record ExampleState : AbstractFluxState
{
    public int Counter { get; init; }
    public Model.WeatherForecast[]? WeatherForecasts { get; init; }
}
