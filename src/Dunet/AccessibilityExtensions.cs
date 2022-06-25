using Microsoft.CodeAnalysis;

namespace Dunet;

internal static class AccessibilityExtensions
{
    private static readonly Dictionary<Accessibility, string> accessibilityKeywords =
        new()
        {
            [Accessibility.Public] = "public",
            [Accessibility.ProtectedOrInternal] = "protected internal",
            [Accessibility.Internal] = "internal",
            [Accessibility.Protected] = "protected",
            [Accessibility.ProtectedAndInternal] = "private protected",
            [Accessibility.Private] = "private",
            [Accessibility.NotApplicable] = "",
        };

    public static string ToKeyword(this Accessibility accessibility) =>
        accessibilityKeywords[accessibility];
}
