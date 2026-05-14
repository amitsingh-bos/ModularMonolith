namespace ModularMonolith.BuildingBlocks.Infrastructure.Options;

public sealed class SmsOptions
{
    public const string SectionName = "Sms";

    /// <summary>Supported values: Console (dev log), Twilio.</summary>
    public string Provider { get; set; } = "Console";

    // Twilio
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}
