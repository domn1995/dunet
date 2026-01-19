namespace Dunet.Generator;

internal static class IdentifierExtensions
{
    extension(string self)
    {
        public string ToMethodParameterCase() =>
            self switch
            {
                // If the identifier starts with '@', it's impossible to collide with a keyword, so we
                // can just return it.
                var s when s.StartsWith("@") => self,
                // If it's any other character:
                // - Prepend '@' to prevent keyword conflicts.
                // - Lowercase the first character to abide by C# style rules for method parameter
                //   casing and prevent collision with its type.
                [var firstCharacter, .. var rest] =>
                    $"@{char.ToLowerInvariant(firstCharacter)}{rest}",
                // If anything else came in, we just return it back and let the caller handle it.
                _ => self,
            };
    }
}
