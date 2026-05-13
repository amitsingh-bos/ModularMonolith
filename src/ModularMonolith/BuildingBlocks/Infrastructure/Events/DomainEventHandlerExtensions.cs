using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Events;

public static class DomainEventHandlerExtensions
{
    /// <summary>
    /// Scans <paramref name="assembly"/> for every class that implements
    /// <see cref="IDomainEventHandler{TEvent}"/> and registers each one as
    /// <c>Scoped</c> under its closed generic interface.
    ///
    /// A single concrete class may implement multiple handler interfaces
    /// (e.g. handle both OrderCreated and OrderCancelled) — each pairing is
    /// registered independently.
    ///
    /// Call once in <c>Program.cs</c>:
    /// <code>
    /// builder.Services.AddDomainEventHandlers(typeof(Program).Assembly);
    /// </code>
    /// </summary>
    public static IServiceCollection AddDomainEventHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        var handlerOpenType = typeof(IDomainEventHandler<>);

        var registrations = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t =>
                t.GetInterfaces()
                 .Where(i => i.IsGenericType &&
                             i.GetGenericTypeDefinition() == handlerOpenType &&
                             typeof(IDomainEvent).IsAssignableFrom(i.GetGenericArguments()[0]))
                 .Select(i => (ConcreteType: t, Interface: i)));

        foreach (var (concreteType, interfaceType) in registrations)
            services.AddScoped(interfaceType, concreteType);

        return services;
    }
}
