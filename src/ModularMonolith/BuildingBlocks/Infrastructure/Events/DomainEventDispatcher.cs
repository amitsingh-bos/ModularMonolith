using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Events;

/// <summary>
/// Resolves and invokes all <see cref="IDomainEventHandler{TEvent}"/> implementations
/// registered in the DI container for the concrete type of each incoming domain event.
///
/// How it works
/// ─────────────
/// 1. <c>DispatchAsync(IDomainEvent)</c> receives the base interface — the concrete type
///    is known only at runtime (e.g. <c>PaymentRefundedDomainEvent</c>).
/// 2. We build the closed generic handler interface:
///    <c>IDomainEventHandler&lt;PaymentRefundedDomainEvent&gt;</c>
/// 3. <c>IServiceProvider.GetServices(closedInterface)</c> returns all handlers registered
///    for that exact event type — zero allocations beyond the list itself.
/// 4. Each handler's <c>HandleAsync</c> method is invoked via a cached
///    <see cref="MethodInfo"/> (one-time reflection per event type, then O(1) lookups).
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    // Cache: event concrete type → MethodInfo of IDomainEventHandler<T>.HandleAsync
    private static readonly ConcurrentDictionary<Type, (Type HandlerInterface, MethodInfo HandleMethod)>
        _cache = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType();

        var (handlerInterface, handleMethod) = _cache.GetOrAdd(eventType, static t =>
        {
            var iface = typeof(IDomainEventHandler<>).MakeGenericType(t);
            // nameof gives us "HandleAsync" without hard-coding the string
            var method = iface.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
            return (iface, method);
        });

        var handlers = _serviceProvider.GetServices(handlerInterface);

        foreach (var handler in handlers)
        {
            _logger.LogDebug("Dispatching {Event} to {Handler}",
                eventType.Name, handler!.GetType().Name);

            await (Task)handleMethod.Invoke(handler, [domainEvent, ct])!;
        }
    }
}
