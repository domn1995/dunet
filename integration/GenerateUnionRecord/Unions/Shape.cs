namespace Dunet.Integration.GenerateUnionRecord.Unions;

[Union]
public partial record class Shape
{
    partial record Rectangle(double Length, double Width);

    partial record Circle(double Radius);

    partial record Triangle(double Base, double Height);
}
