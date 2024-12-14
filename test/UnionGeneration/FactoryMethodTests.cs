namespace Dunet.Test.UnionGeneration;

public class FactoryMethodTests
{
    [Fact]
    public void NonGenericFactoryMethods()
    {
        var programCs = """
            using Dunet;
            using System;

            Result success = Result.AsSuccess("Hello, world!");
            Result error = Result.AsFailure(new Exception("Boom!"), "Something went wrong :(");

            [Union]
            partial record Result
            {
                partial record Success(string Value);
                partial record Failure(Exception Error, string Reason);
            }
            """;

        // Act.
        var result = Compiler.Compile(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void GenericFactoryMethods()
    {
        var programCs = """
            using Dunet;
            using System;

            Result<string> success = Result<string>.AsSuccess("Hello, world!");
            Result<string> error = Result<string>.AsFailure(new Exception("Boom!"), "Something went wrong :(");

            [Union]
            partial record Result<T>
            {
                partial record Success(T Value);
                partial record Failure(Exception Error, string Reason);
            }
            """;

        // Act.
        var result = Compiler.Compile(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
