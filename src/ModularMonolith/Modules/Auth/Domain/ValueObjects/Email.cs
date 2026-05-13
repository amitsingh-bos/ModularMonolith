using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;
using System.Text.RegularExpressions;

namespace ModularMonolith.Modules.Auth.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");

        var normalized = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new DomainException($"'{email}' is not a valid email address.");

        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
