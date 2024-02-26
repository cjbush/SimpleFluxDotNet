namespace SimpleFluxDotNet.Example.States;

public sealed record ExampleState : AbstractFluxState
{
    public int Counter { get; init; }
}
