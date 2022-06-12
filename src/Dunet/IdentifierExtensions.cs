namespace Dunet;

internal static class IdentifierExtensions
{
    public static string ToPropertyCase(this string identifier)
    {
        if (identifier[0] == '@')
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
}
