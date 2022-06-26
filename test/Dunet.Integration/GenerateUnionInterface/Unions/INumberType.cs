namespace Dunet.Integration.GenerateUnionInterface.Unions;

[Union]
interface INumberType
{
    void Rational(int numerator, int denominator);
    void Irrational(double value);
}
