namespace Dunet.Integration.GenerateUnionRecord.Unions;

[Union]
public partial record NumberType
{
    partial record Rational(int Numerator, int Denominator);

    partial record Irrational(double Value);
}
