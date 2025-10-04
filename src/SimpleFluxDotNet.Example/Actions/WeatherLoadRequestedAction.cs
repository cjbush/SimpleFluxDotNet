using System.Net.Http.Json;
using SimpleFluxDotNet.Abstractions;
using SimpleFluxDotNet.Example.States;

namespace SimpleFluxDotNet.Example.Actions;

public sealed record WeatherLoadRequestedAction() : IFluxAction;
public sealed record WeatherLoadedAction(Model.WeatherForecast[] Forecasts) : IFluxAction;


public sealed class LoadWeatherMiddleware : IFluxMiddleware<ExampleState, WeatherLoadRequestedAction>
{
    private readonly HttpClient _httpClient;

    public LoadWeatherMiddleware(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DispatchAsync(WeatherLoadRequestedAction action, FluxMiddlewareContext<ExampleState> context, CancellationToken ct = default)
    {
        var forecasts = await _httpClient.GetFromJsonAsync<Model.WeatherForecast[]>("sample-data/weather.json", ct);
        if (forecasts is not null)
        {
            await context.Dispatch(new WeatherLoadedAction(forecasts));
        }
    }
}
