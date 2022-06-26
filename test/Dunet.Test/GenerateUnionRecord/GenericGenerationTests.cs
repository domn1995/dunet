using Dunet.Test.Runtime;

namespace Dunet.Test.GenerateUnionRecord;

public class GenericGenerationTests : UnionRecordTests
{
    [Fact]
    public void UnionTypeMayHaveGenericParameter()
    {
        var programCs =
            @"
using Dunet;

Option<int> some = new Option<int>.Some(1);
Option<int> none = new Option<int>.None();

[Union]
partial record Option<T>
{
    partial record Some(T Value);
    partial record None();
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionMemberMayHaveGenericParameter()
    {
        var programCs =
            @"
using Dunet;

Option some = new Option.Some<int>(1);
Option none = new Option.None();

[Union]
partial record Option
{
    partial record Some<T>(T Value);
    partial record None();
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1, 2, "0.5")]
    [InlineData(1, 0, "Error: division by zero.")]
    public void CanReturnImplementationsOfGenericUnion(
        int dividend,
        int divisor,
        string expectedOutput
    )
    {
        var programCs =
            @$"
using Dunet;

static string GetOutputValue() => Divide() switch
{{
    Option.Some<double> some => some.Value.ToString(),
    Option.None none => ""Error: division by zero."",
    _ => throw new System.InvalidOperationException(),
}};

static Option Divide()
{{
    var dividend = {dividend};
    var divisor = {divisor};

    if (divisor is 0)
    {{
        return new Option.None();
    }}

    return new Option.Some<double>((double)dividend / divisor);
}}

[Union]
partial record Option
{{
    partial record Some<T>(T Value);
    partial record None();
}}";

        // Act.
        var result = Compile.ToAssembly(programCs);
        var actualArea = result.Assembly.ExecuteStaticMethod<string>("GetOutput");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedOutput);
    }
}
