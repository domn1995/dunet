namespace Dunet.Test.UnionGeneration;

public class FactoryMethodTests
{
    [Fact]
    public void NonGenericFactoryMethods()
    {
        var programCs = """
            using Dunet;
            using System;

            Result success = Result.OfSuccess("Hello, world!");
            Result error = Result.OfFailure(new Exception("Boom!"), "Something went wrong :(");

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

            Result<string> success = Result<string>.OfSuccess("Hello, world!");
            Result<string> error = Result<string>.OfFailure(new Exception("Boom!"), "Something went wrong :(");

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

    [Fact]
    public void KeywordsNamedProperties()
    {
        var programCs = """
            using Dunet;
            using System;

            Result success = Result.OfInner1("base", "string");
            Result error = Result.OfInner2("class");

            [Union]
            partial record Result
            {
                partial record Inner1(string Base, string String);
                partial record Inner2(string Class);
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
    public void ParentLevelProperties()
    {
        var programCs = """
            using Dunet;
            using System;

            Result success = Result.OfInner1("base", "string", "name");
            Result error = Result.OfInner2("class", "name");

            [Union]
            partial record Result
            {
                public required string Name { get; init; }
                partial record Inner1(string Base, string String);
                partial record Inner2(string Class);
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
