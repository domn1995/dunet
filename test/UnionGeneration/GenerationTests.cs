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

    [Fact]
    public void GenericUnionsWithTheSameName()
    {
        // Arrange.
        var programCs = """
            using Dunet;
            using System;

            Result<Guid, Exception> result1 = Result.Ok<Guid, Exception>(Guid.NewGuid());
            Result<Exception> result2 = Result.Error(new Exception("Boom!"));

            [Union]
            public partial record Result<T, TError>
            {
                public partial record Ok(T Value);
                public partial record Error(TError Value);
            }

            [Union]
            public partial record Result<TError>
            {
                public partial record Ok();
                public partial record Error(TError Value);
            }

            public static class Result
            {
                public static Result<T, TError> Ok<T, TError>(T value) => new Result<T, TError>.Ok(value);

                public static Result<T, TError> Error<T, TError>(TError value) => new Result<T, TError>.Error(value);

                public static Result<TError> Ok<TError>() => new Result<TError>.Ok();

                public static Result<TError> Error<TError>(TError value) => new Result<TError>.Error(value);
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
