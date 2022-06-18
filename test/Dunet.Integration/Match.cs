using Dunet.Integration.Unions;

namespace Dunet.Integration;

public class Match
{
    [Fact]
    public void Rational()
    {
        var number = new Rational(1, 2);
        var formatted = Format(number);
        formatted.Should().Be("1/2");
    }

    [Fact]
    public void Irrational()
    {
        var number = new Irrational(1.23);
        var formatted = Format(number);
        formatted.Should().Be("1.23");
    }

    private static string Format(INumberType number) =>
        number.Match(
            rational => $"{rational.Numerator}/{rational.Denominator}",
            irrational => irrational.Value.ToString()
        );
}
