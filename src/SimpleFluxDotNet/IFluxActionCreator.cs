namespace SimpleFluxDotNet;

public interface IFluxActionCreator<TAction> where TAction : class, IFluxAction
{
    Task<TAction> CreateAsync(CancellationToken ct = default);
}
