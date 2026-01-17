namespace Dunet.Test.UnionGeneration;

public sealed class MultipleGenericUnionsGenerationTests
{
    [Fact]
    public void TwoGenericUnionsWithSameNameDifferentTypeParameters()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            Result<string> result1 = new Result<string>.Ok("Success");
            Result<int, string> result2 = new Result<int, string>.Ok(42);

            [Union]
            public partial record Result<T>
            {
                public partial record Ok(T Value);
                public partial record Error();
            }

            [Union]
            public partial record Result<T, TError>
            {
                public partial record Ok(T Value);
                public partial record Error(TError ErrorValue);
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
    public void ThreeGenericUnionsWithSameNameIncreasingTypeParameters()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            Response<string> response1 = new Response<string>.Success("data");
            Response<string, int> response2 = new Response<string, int>.Success("data");
            Response<string, int, bool> response3 = new Response<string, int, bool>.Success("data");

            [Union]
            public partial record Response<T>
            {
                public partial record Success(T Data);
                public partial record Failure();
            }

            [Union]
            public partial record Response<T, TError>
            {
                public partial record Success(T Data);
                public partial record Failure(TError Error);
            }

            [Union]
            public partial record Response<T, TError, TMetadata>
            {
                public partial record Success(T Data);
                public partial record Failure(TError Error);
                public partial record Pending(TMetadata Metadata);
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
    public void MultipleGenericUnionsWithSameNameInDifferentNamespaces()
    {
        // Arrange.
        var resultCS = """
            using Dunet;

            namespace Results;

            [Union]
            public partial record Result<T>
            {
                public partial record Ok(T Value);
                public partial record Error();
            }

            [Union]
            public partial record Result<T, TError>
            {
                public partial record Ok(T Value);
                public partial record Error(TError ErrorValue);
            }
            """;

        var programCs = """
            using Results;

            var result1 = new Result<string>.Ok("Success");
            var result2 = new Result<int, string>.Ok(42);
            """;

        // Act.
        var result = Compiler.Compile(resultCS, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void MultipleGenericUnionsWithSameNameWithComplexVariants()
    {
        // Arrange.
        var programCs = """
            using Dunet;
            using System;

            Operation<int> op1 = new Operation<int>.Success(42);
            Operation<string, Exception> op2 = new Operation<string, Exception>.Success("done");

            [Union]
            public partial record Operation<T>
            {
                public partial record Success(T Result);
                public partial record InProgress(double PercentComplete);
                public partial record Cancelled();
            }

            [Union]
            public partial record Operation<T, TError>
            {
                public partial record Success(T Result);
                public partial record Failed(TError Error);
                public partial record Cancelled();
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
    public void MultipleGenericUnionsWithSameNameCanInstantiateAllVariants()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            // Result<T>
            Result<int> r1_ok = new Result<int>.Ok(42);
            Result<int> r1_error = new Result<int>.Error();

            // Result<T, TError>
            Result<string, string> r2_ok = new Result<string, string>.Ok("success");
            Result<string, string> r2_error = new Result<string, string>.Error("failed");

            [Union]
            public partial record Result<T>
            {
                public partial record Ok(T Value);
                public partial record Error();
            }

            [Union]
            public partial record Result<T, TError>
            {
                public partial record Ok(T Value);
                public partial record Error(TError ErrorValue);
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
    public void MultipleGenericUnionsWithSameNameWithConstraints()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            Container<string> c1 = new Container<string>.Filled("data");
            Container<int, string> c2 = new Container<int, string>.Filled(42);

            [Union]
            public partial record Container<T> where T : class
            {
                public partial record Filled(T Data);
                public partial record Empty();
            }

            [Union]
            public partial record Container<T, TMetadata> where T : struct
            {
                public partial record Filled(T Data);
                public partial record Empty(TMetadata Metadata);
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
