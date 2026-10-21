using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Web.Api.Features.Builds;

// Ids in share links: random, so nobody can walk through other people's builds by counting
public static class BuildIds
{
    // 64 bits: 11 URL-safe characters, collisions out of the question at this scale
    private const int IdBytes = 8;

    public static string New() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(IdBytes));

    // Rejects anything that couldn't have come from New() before it reaches storage
    public static bool IsWellFormed(string? id) =>
        id is { Length: 11 } && id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
}
