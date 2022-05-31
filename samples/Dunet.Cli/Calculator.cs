namespace Dunet.Cli;

public class Calculator
{
    public static double GetArea(IShape shape) => shape switch
    {
        Rectangle rect => rect.Length * rect.Width,
        Circle circle => 2.0 * Math.PI * circle.Radius,
        Triangle triangle => 1.0 / 2.0 * triangle.Base * triangle.Height,
        _ => 0d,
    };
}