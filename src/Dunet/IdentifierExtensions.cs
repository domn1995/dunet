namespace Dunet;

internal static class IdentifierExtensions
{
    public static string ToPropertyCase(this string identifier)
    {
        if (identifier[0] is '@')
        {
            identifier = identifier.Substring(1);
        }

        var capitalizedFirstChar = char.ToUpper(identifier[0]).ToString();

        if (identifier.Length < 2)
        {
            return capitalizedFirstChar;
        }

        return $"{capitalizedFirstChar}{identifier.Substring(1)}";
    }

    public static string ToMethodParameterCase(this string identifier)
    {
        if (identifier[0] is '@')
        {
            return identifier;
        }

        var isFirstCharacterLowercased = char.IsLower(identifier[0]);

        if (isFirstCharacterLowercased)
        {
            return identifier;
        }

        var lowercasedFirstCharacter = char.ToLower(identifier[0]).ToString();

        if (identifier.Length < 2)
        {
            return lowercasedFirstCharacter;
        }

        return $"{lowercasedFirstCharacter}{identifier.Substring(1)}";
    }
}
