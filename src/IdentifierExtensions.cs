﻿namespace Dunet;

internal static class IdentifierExtensions
{
    public static string ToMethodParameterCase(this string identifier) =>
        identifier switch
        {
            // If the identifier starts with '@', it's impossible to collide with a keyword, so we
            // can just return it.
            ['@', .. _] => identifier,
            // If it's any other character:
            // - Prepend '@' to prevent keyword conflicts.
            // - Lowercase the first character to abide by C# style rules for method parameter
            //   casing and prevent collision with its type.
            [var firstCharacter, .. var rest] => $"@{char.ToLowerInvariant(firstCharacter)}{rest}",
            // If an empty identifier came in, we just return it back and let the caller handle it.
            { Length: 0 } => identifier,
        };
}
