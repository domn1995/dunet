namespace Dunet.Test.GenerateUnionRecord;

public class ImplicitConversionTests : UnionRecordTests
{
    [Fact]
    public void UnionMemberInnerValuesAreAssignableToUnionType()
    {
        var programCs =
            @"
using Dunet;
using System;

Result success = ""Hello, world!"";
Result error = new Exception(""Boom!"");

[Union]
partial record Result
{
    partial record Success(string Value);
    partial record Failure(Exception Error);
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionMemberInnerValueSubclassesAreAssignableToUnionType()
    {
        var programCs =
            @"
using Dunet;
using System;
using System.Collections.Generic;

Result success = new StringList() { ""foo"", ""bar"", ""baz"" };
Result error = new InvalidOperationException(""Boom!"");

class StringList : List<string> {}

[Union]
partial record Result
{
    partial record Success(List<string> Values);
    partial record Failure(Exception Error);
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionMemberGenericInnerValuesAreAssignableToUnionType()
    {
        var programCs =
            @"
using Dunet;
using System;
using static Result<string, int>;

Result<string, int> success = 42;
Result<string, int> error = ""Something went wrong."";

[Union]
partial record Result<TFailure, TSuccess>
{
    partial record Success(TSuccess Value);
    partial record Failure(TFailure Error);
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
