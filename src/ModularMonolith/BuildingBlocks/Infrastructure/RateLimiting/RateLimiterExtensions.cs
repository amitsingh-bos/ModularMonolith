using System.Security.Claims;
using System.Threading.RateLimiting;

namespace ModularMonolith.BuildingBlocks.Infrastructure.RateLimiting;

/// <summary>
/// Registers the application's rate-limiting policies.
/// </summary>
/// <remarks>
/// Two named policies are configured:
///
/// <b>"auth" — Fixed Window (5 requests / 60 s per IP)</b><br/>
/// A fixed window divides time into equal, non-overlapping buckets.
/// Each bucket starts fresh; the counter resets the moment a new window begins,
/// regardless of when inside the previous window the requests arrived.
///
/// Example — permit limit 3, window 60 s:
/// <code>
/// Timeline  |  0s        20s       40s  |  60s       80s       100s |
/// Requests  |  ●         ●         ●    |  ●         ●              |
/// Allowed   |  ✓         ✓         ✓    |  ✓         ✓              |
/// --         burst of 3 at t=50s would be BLOCKED until t=60s resets the window
/// </code>
/// Best for: anonymous endpoints where a simple absolute cap per time unit is enough
/// (login, register, refresh).  Partition key = remote IP address.
///
/// <b>"api" — Sliding Window (100 requests / 60 s per user, 6 segments)</b><br/>
/// A sliding window keeps a rolling view of the last <c>Window</c> duration by
/// splitting it into <c>SegmentsPerWindow</c> fixed mini-buckets.  As each segment
/// expires it is subtracted from the running total, so the effective window moves
/// with the clock rather than resetting all at once.
///
/// Example — permit limit 6, window 60 s, 6 segments (one 10-s segment per slot):
/// <code>
/// Segments  | [s0]  [s1]  [s2]  [s3]  [s4]  [s5] |  [s0]  ...
/// Counts    |  2     1     0     1     1     1    |   2    ...
/// Running Σ |  6  → 5  → 5  → 4  → 3  → 2  → 4  ...
/// </code>
/// A burst that exhausts all 6 tokens is throttled, but tokens are freed
/// segment-by-segment rather than all at once, giving a smoother experience.
/// Best for: authenticated endpoints where fair per-user throttling matters.
/// Partition key = JWT <c>sub</c> claim (falls back to remote IP for anonymous callers).
/// </remarks>
public static class RateLimiterExtensions
{
    /// <summary>
    /// Adds the "auth" (fixed window) and "api" (sliding window) rate-limit policies
    /// and configures a JSON <c>429 Too Many Requests</c> rejection response.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                    ? (int)retry.TotalSeconds
                    : 60;
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
                await context.HttpContext.Response.WriteAsync(
                    $$"""{"success":false,"message":"Too many requests. Retry after {{retryAfter}} seconds.","statusCode":429}""",
                    ct);
            };

            // Fixed window — 5 requests per minute per IP (auth endpoints)
            options.AddPolicy("auth", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Sliding window — 100 requests per minute per user (authenticated endpoints)
            options.AddPolicy("api", httpContext =>
            {
                var userId = httpContext.User.FindFirstValue("sub");
                var key = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(key,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
