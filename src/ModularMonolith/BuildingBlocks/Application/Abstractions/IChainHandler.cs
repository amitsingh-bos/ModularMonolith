namespace ModularMonolith.BuildingBlocks.Application.Abstractions;

public interface IChainHandler<TContext>
{
    IChainHandler<TContext> SetNext(IChainHandler<TContext> next);
    Task HandleAsync(TContext context, CancellationToken ct = default);
}

public abstract class ChainHandlerBase<TContext> : IChainHandler<TContext>
{
    private IChainHandler<TContext>? _next;

    public IChainHandler<TContext> SetNext(IChainHandler<TContext> next)
    {
        _next = next;
        return next;
    }

    protected Task NextAsync(TContext context, CancellationToken ct) =>
        _next?.HandleAsync(context, ct) ?? Task.CompletedTask;

    public abstract Task HandleAsync(TContext context, CancellationToken ct);
}
