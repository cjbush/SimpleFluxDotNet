using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleFluxDotNet.Example;
using SimpleFluxDotNet.Example.Actions;
using SimpleFluxDotNet.Example.Reducers;
using SimpleFluxDotNet.Example.States;
using SimpleFluxDotNet.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddFluxStateManagement(flux =>
{
    flux.ConfigureStateContainer<ExampleState>(state =>
    {
        state.HandleAction<IncrementCounterButtonClickedAction>(action => action.UseReducer<IncrementCounterReducer>());
        state.HandleAction<WeatherLoadRequestedAction>(action => action.UseMiddleware<LoadWeatherMiddleware>());
        state.HandleAction<WeatherLoadedAction>(action =>
        {
            action.UseReducer(static (action, state) => state with
            {
                WeatherForecasts = action.Forecasts
            });
        });
    });
});


await builder.Build().RunAsync();
