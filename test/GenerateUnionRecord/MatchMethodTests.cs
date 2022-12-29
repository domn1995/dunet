using Dunet.Test.Compiler;
using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionRecord;

public class MatchMethodTests : UnionRecordTests
{
    [Fact]
    public void CanUseUnionTypesInDedicatedMatchMethod()
    {
        // Arrange.
        var source = """
using Dunet;

Shape shape = new Shape.Rectangle(3, 4);

var area = shape.Match(
    circle => 3.14 * circle.Radius * circle.Radius,
    rectangle => rectangle.Length * rectangle.Width,
    triangle => triangle.Base * triangle.Height / 2
);

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}
""";
        // Act.
        var result = Compile.ToAssembly(source);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Shape shape = new Shape.Rectangle(3, 4);", 12d)]
    [InlineData("Shape shape = new Shape.Circle(1);", 3.14d)]
    [InlineData("Shape shape = new Shape.Triangle(4, 2);", 4d)]
    public void MatchMethodCallsCorrectFunctionArgument(
        string shapeDeclaration,
        double expectedArea
    )
    {
        // Arrange.
        var source = $$"""
using Dunet;

static double GetArea()
{
    {{shapeDeclaration}}
    return shape.Match(
        circle => 3.14 * circle.Radius * circle.Radius,
        rectangle => rectangle.Length * rectangle.Width,
        triangle => triangle.Base * triangle.Height / 2
    );
}

[Union]
partial record Shape
{
    partial record Circle(double Radius);
    partial record Rectangle(double Length, double Width);
    partial record Triangle(double Base, double Height);
}
""";
        // Act.
        var result = Compile.ToAssembly(source);
        var actualArea = result.Assembly?.ExecuteStaticMethod<double>("GetArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedArea);
    }

    [Theory]
    [InlineData("Keyword keyword = new Keyword.New();", "new")]
    [InlineData("Keyword keyword = new Keyword.Base();", "base")]
    [InlineData("Keyword keyword = new Keyword.Null();", "null")]
    public void CanMatchOnUnionMembersNamedAfterKeywords(
        string keywordDeclaration,
        string expectedKeyword
    )
    {
        // Arrange.
        var source = $$"""
using Dunet;

static string GetKeyword()
{
    {{keywordDeclaration}}
    return keyword.Match(
        @new => "new",
        @base => "base",
        @null => "null"
    );
}

[Union]
partial record Keyword
{
    partial record New;
    partial record Base;
    partial record Null;
}
""";
        // Act.
        var result = Compile.ToAssembly(source);
        var actualKeyword = result.Assembly?.ExecuteStaticMethod<string>("GetKeyword");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualKeyword.Should().Be(expectedKeyword);
    }
}
