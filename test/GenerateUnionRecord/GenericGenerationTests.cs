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
    public void UnionMemberMayNotHaveGenericParameter()
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
        var errorMessages = result.CompilationErrors.Select(error => error.GetMessage());

        // Assert.
        result.Assembly.Should().BeNull();
        result.CompilationErrors.Should().NotBeEmpty();
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
using System.Globalization;

static string GetActualArea() => Divide() switch
{{
    Option<double>.Some some => some.Value.ToString(CultureInfo.InvariantCulture),
    Option<double>.None none => ""Error: division by zero."",
    _ => throw new System.InvalidOperationException(),
}};

static Option<double> Divide()
{{
    var dividend = {dividend};
    var divisor = {divisor};

    if (divisor is 0)
    {{
        return new Option<double>.None();
    }}

    return new Option<double>.Some((double)dividend / divisor);
}}

[Union]
partial record Option<T>
{{
    partial record Some(T Value);
    partial record None();
}}";

        // Act.
        var result = Compile.ToAssembly(programCs);
        var actualArea = result.Assembly?.ExecuteStaticMethod<string>("GetActualArea");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        actualArea.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("Success(\"Successful!\")", "Successful!")]
    [InlineData("Failure(new Exception(\"Failure!\"))", "Failure!")]
    public void CanReturnImplementationsOfGenericUnionWithMultipleTypeParameters(
        string resultRecord,
        string expectedMessage
    )
    {
        var programCs =
            @$"
using System;
using Dunet;

static Result<Exception, string> DoWork() => new Result<Exception, string>.{resultRecord};

static string GetActualMessage() => DoWork().Match(
    success => success.Value,
    failure => failure.Error.Message
);

[Union]
partial record Result<TFailure, TSuccess>
{{
    partial record Success(TSuccess Value);
    partial record Failure(TFailure Error);
}}";
        // Act.
        var result = Compile.ToAssembly(programCs);
        var actualMessage = result.Assembly?.ExecuteStaticMethod<string>("GetActualMessage");

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
        actualMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void SupportsGenericTypeArgumentConstraints()
    {
        var programCs =
            @$"
using System;
using Dunet;

var result = new Result<string, string>.Success(""Can't do this."");

[Union]
partial record Result<TFailure, TSuccess> where TFailure : Exception
{{
    partial record Success(TSuccess Value);
    partial record Failure(TFailure Error);
}}";
        // Act.
        var result = Compile.ToAssembly(programCs);
        var errorMessages = result.CompilationErrors.Select(error => error.GetMessage());

        // Assert.
        result.Assembly.Should().BeNull();
        errorMessages
            .Should()
            .ContainSingle(
                "The type 'string' cannot be used as type parameter 'TFailure' in the "
                    + "generic type or method 'Result<TFailure, TSuccess>'.There is no "
                    + "implicit reference conversion from 'string' to 'System.Exception'."
            );
    }
}
