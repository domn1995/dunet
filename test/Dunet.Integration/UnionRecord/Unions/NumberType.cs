namespace Dunet.Integration.UnionRecord.Unions;

[Union]
public partial record NumberType
{
    partial record Rational(int Numberator, int Denominator);

    partial record Irrational(double Value);
}
