namespace Dunet.Test.UnionGeneration;

public sealed class GenerationTests
{
    [Fact]
    public void UnionVariantsExtendUnionType()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            QueryState loading = new QueryState.Loading();
            QueryState success = new QueryState.Success();
            QueryState error = new QueryState.Error();

            [Union]
            partial record QueryState
            {
                partial record Loading();
                partial record Success();
                partial record Error();
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
    public void UnionVariantsMayHaveNoProperties()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            var loading = new QueryState.Loading();
            var success = new QueryState.Success();
            var error = new QueryState.Error();

            [Union]
            partial record QueryState
            {
                partial record Loading();
                partial record Success();
                partial record Error();
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
    public void UnionMayContainSingleVariant()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            var single = new Single.OnlyVariant();

            [Union]
            partial record Single
            {
                partial record OnlyVariant();
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
    public void UnionMayBeEmpty()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            // Must have something for top level program to compile.
            var dummy = 1;

            [Union]
            partial record Empty;
            """;

        // Act.
        var result = Compiler.Compile(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionTypeMayHaveComplexVariants()
    {
        // Arrange.
        var programCs = """
            using Dunet;
            using System;

            var success = new Result.Success(Guid.NewGuid());
            var failure = new Result.Failure(new Exception("Boom!"));

            [Union]
            partial record Result
            {
                partial record Success(Guid Id);
                partial record Failure(Exception Error);
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
    public void UnionTypeMayHaveComplexVariantsFromOtherNamespace()
    {
        // Arrange.
        var dataCs = """
            namespace ComplexTypes;

            public record Data(string Value);
            """;

        var resultCs = """
            using Dunet;
            using ComplexTypes;
            using System;

            namespace Results;

            [Union]
            partial record Result
            {
                partial record Success(Data Value);
                partial record Failure(Exception Error);
            }
            """;

        var programCs = """
            using System;
            using ComplexTypes;
            using Results;

            // Must have something for top level program to execute.
            var success = new Result.Success(new Data("foo"));
            var failure = new Result.Failure(new Exception("bar"));
            """;

        // Act.
        var result = Compiler.Compile(dataCs, resultCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionTypeMayHaveRequiredProperties()
    {
        // Arrange.
        var programCs = """
            using Dunet;
            using System;

            Result result1 = new Result.Success(Guid.NewGuid()) { Name = "Success" };
            Result result2 = new Result.Failure(new Exception("Boom!")) { Name = "Failure" };

            var result1Name = result1.Name;
            var result2Name = result2.Name;

            [Union]
            partial record Result
            {
                public required string Name { get; init; }
                partial record Success(Guid Id);
                partial record Failure(Exception Error);
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
