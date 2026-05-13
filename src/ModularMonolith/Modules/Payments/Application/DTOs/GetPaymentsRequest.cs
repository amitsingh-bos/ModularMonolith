using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Payments.Domain.Enums;

namespace ModularMonolith.Modules.Payments.Application.DTOs;

public class GetPaymentsRequest : PagedQuery
{
    public PaymentStatus? Status { get; init; }
    public Guid? OrderId { get; init; }
    public PaymentMethod? Method { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
