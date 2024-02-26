using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleFluxDotNet;
using SimpleFluxDotNet.Example;
using SimpleFluxDotNet.Example.Events;
using SimpleFluxDotNet.Example.Reducers;
using SimpleFluxDotNet.Example.States;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


builder.Services.AddFluxStateManagement(flux =>
{
    flux.ForState<ExampleState>()
            .HandleEvent<IncrementCounterButtonClickedEvent>().With<IncrementCounterReducer>();
});


await builder.Build().RunAsync();
