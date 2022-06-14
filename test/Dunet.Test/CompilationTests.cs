namespace Dunet.Test;

public class CompilationTests
{
    [Fact]
    public void Compiles()
    {
        // Arrange.
        var source =
            @"
namespace Dunet.Cli;

[Union]
public interface IShape
{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}

[Union]
public interface IChoice
{
    void Yes();
    void No();
}";
        // Act.
        var (_, generationDiagnostics, compilationDiagnostics) = Compile.ToAssembly(source);

        // Assert.
        generationDiagnostics.Should().BeEmpty();
        compilationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void GeneratesUnionTypesInSameNamespace()
    {
        // Arrange.
        const string expectedNamespace = "Test.Namespace";
        var source =
            @$"
using Dunet;

namespace {expectedNamespace};

[Union]
public interface IShape
{{
    IShape Circle(double radius);
    IShape Rectangle(double length, double width);
    IShape Triangle(double @base, double height);
}}

[Union]
public interface IChoice
{{
    void Yes();
    void No();
}}";

        // Act.
        var (_, generationDiagnostics, compilationDiagnostics) = Compile.ToAssembly(source);

        // Assert.
        generationDiagnostics.Should().BeEmpty();
        compilationDiagnostics.Should().BeEmpty();
    }
}
