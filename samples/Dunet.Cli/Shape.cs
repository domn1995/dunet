namespace Dunet.Shapes;

[Union]
public partial record Shape
{
    partial record Circle(double Radius);

    partial record Rectangle(double Length, double Width);

    partial record Triangle(double Base, double Height);
}
