using Microsoft.AspNetCore.Components;

namespace SimpleFluxDotNet.Abstractions;

public abstract class FluxComponent<TState> : ComponentBase where TState : AbstractFluxState
{
    [Inject] protected IFluxStateStore<TState> State { get; private set; } = default!;

    protected override Task OnInitializedAsync()
    {
        State.OnStateChanged += (sender, args, ct) => InvokeAsync(StateHasChanged);
        return base.OnInitializedAsync();
    }

}
