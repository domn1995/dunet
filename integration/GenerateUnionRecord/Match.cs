using System.Globalization;
using Dunet.Integration.GenerateUnionRecord.Unions;

namespace Dunet.Integration.GenerateUnionRecord;

public class Match
{
    [Fact]
    public void Rational()
    {
        var number = new NumberType.Rational(1, 2);
        var formatted = Format(number);
        formatted.Should().Be("1/2");
    }

    [Fact]
    public void Irrational()
    {
        var number = new NumberType.Irrational(1.23);
        var formatted = Format(number);
        formatted.Should().Be("1.23");
    }

    private static string Format(NumberType number) =>
        number.Match(
            rational => $"{rational.Numerator}/{rational.Denominator}",
            irrational => irrational.Value.ToString(CultureInfo.InvariantCulture)
        );
}
