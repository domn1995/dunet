namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class MultipleGenericUnionsExtensionsTests
{
    [Fact]
    public void CanGenerateMatchExtensionsForTwoGenericUnionsWithSameName()
    {
        // Arrange.
        var resultCs = """
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

            var result1 = new Result<int>.Ok(42);
            var value1 = result1.Match(
                ok => ok.Value * 2,
                error => 0
            );

            var result2 = new Result<string, string>.Ok("success");
            var value2 = result2.Match(
                ok => ok.Value.Length,
                error => -1
            );
            """;

        // Act.
        var result = Compiler.Compile(resultCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Fact]
    public void CanGenerateMatchExtensionsForThreeGenericUnionsWithSameName()
    {
        // Arrange.
        var responseCs = """
            using Dunet;

            namespace Responses;

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

        var programCs = """
            using Responses;

            var resp1 = new Response<int>.Success(10);
            var val1 = resp1.Match(
                success => success.Data + 5,
                failure => 0
            );

            var resp2 = new Response<string, int>.Success("hello");
            var val2 = resp2.Match(
                success => success.Data.Length,
                failure => -failure.Error
            );

            var resp3 = new Response<bool, string, double>.Success(true);
            var val3 = resp3.Match(
                success => success.Data ? 1 : 0,
                failure => -1,
                pending => (int)pending.Metadata
            );
            """;

        // Act.
        var result = Compiler.Compile(responseCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Task")]
    [InlineData("ValueTask")]
    public void CanGenerateMatchAsyncExtensionsForMultipleGenericUnionsWithSameName(string taskType)
    {
        // Arrange.
        var resultCs = """
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

        var programCs = $$"""
            using System.Threading.Tasks;
            using Results;

            async Task Test1()
            {
                {{taskType}}<Result<int>> task1 = {{taskType}}.FromResult<Result<int>>(new Result<int>.Ok(42));
                var value1 = await task1.MatchAsync(
                    ok => Task.FromResult(ok.Value * 2),
                    error => Task.FromResult(0)
                );
            }

            async Task Test2()
            {
                {{taskType}}<Result<string, string>> task2 = {{taskType}}.FromResult<Result<string, string>>(new Result<string, string>.Ok("success"));
                var value2 = await task2.MatchAsync(
                    ok => Task.FromResult(ok.Value.Length),
                    error => Task.FromResult(-1)
                );
            }
            """;

        // Act.
        var result = Compiler.Compile(resultCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Fact]
    public void MatchExtensionsCorrectlyRouteToDifferentVariants()
    {
        // Arrange.
        var resultCs = """
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

            // Test Result<T> - Ok variant
            Result<int> r1 = new Result<int>.Ok(100);
            var r1_result = r1.Match(
                ok => ok.Value * 10,
                error => -1
            );
            System.Console.WriteLine(r1_result == 1000);

            // Test Result<T> - Error variant
            Result<int> r2 = new Result<int>.Error();
            var r2_result = r2.Match(
                ok => ok.Value * 10,
                error => -1
            );
            System.Console.WriteLine(r2_result == -1);

            // Test Result<T, TError> - Ok variant
            Result<string, string> r3 = new Result<string, string>.Ok("hello");
            var r3_result = r3.Match(
                ok => ok.Value.Length,
                error => -error.ErrorValue.Length
            );
            System.Console.WriteLine(r3_result == 5);

            // Test Result<T, TError> - Error variant
            Result<string, string> r4 = new Result<string, string>.Error("failure");
            var r4_result = r4.Match(
                ok => ok.Value.Length,
                error => -error.ErrorValue.Length
            );
            System.Console.WriteLine(r4_result == -7);
            """;

        // Act.
        var result = Compiler.Compile(resultCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Fact]
    public void MatchExtensionsWorkWithVoidReturnType()
    {
        // Arrange.
        var resultCs = """
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

            Result<int> r1 = new Result<int>.Ok(42);
            r1.Match(
                ok => { System.Console.WriteLine($"Success: {ok.Value}"); },
                error => { System.Console.WriteLine("Error"); }
            );

            Result<string, string> r2 = new Result<string, string>.Error("oops");
            r2.Match(
                ok => { System.Console.WriteLine($"Success: {ok.Value}"); },
                error => { System.Console.WriteLine($"Error: {error.ErrorValue}"); }
            );
            """;

        // Act.
        var result = Compiler.Compile(resultCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Fact]
    public void MultipleGenericUnionsWithSameName_EachGetUniqueExtensions()
    {
        // Arrange.
        var statusCs = """
            using Dunet;

            namespace Status;

            [Union]
            public partial record Status<T>
            {
                public partial record Active(T Info);
                public partial record Inactive();
            }

            [Union]
            public partial record Status<T, TReason>
            {
                public partial record Active(T Info);
                public partial record Inactive(TReason Reason);
            }
            """;

        var programCs = """
            using Status;

            var s1 = new Status<int>.Active(10);
            // Each should have unique Match extension based on variant count
            var r1 = s1.Match(a => a.Info, i => 0);

            var s2 = new Status<string, string>.Inactive("paused");
            // This should have different Match extension signature
            var r2 = s2.Match(a => a.Info.Length, i => i.Reason.Length);
            """;

        // Act.
        var result = Compiler.Compile(statusCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Task")]
    [InlineData("ValueTask")]
    public void MatchAsyncExtensionsWorkWithAsyncLambdas(string taskType)
    {
        // Arrange.
        var resultCs = """
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

        var programCs = $$"""
            using System.Threading.Tasks;
            using Results;

            {{taskType}}<Result<int>> task1 = {{taskType}}.FromResult<Result<int>>(new Result<int>.Ok(42));
            var value1 = await task1.MatchAsync(
                async ok => { await Task.Delay(0); return ok.Value * 2; },
                async error => { await Task.Delay(0); return 0; }
            );

            {{taskType}}<Result<string, string>> task2 = {{taskType}}.FromResult<Result<string, string>>(new Result<string, string>.Ok("hello"));
            var value2 = await task2.MatchAsync(
                async ok => { await Task.Delay(0); return ok.Value.Length; },
                async error => { await Task.Delay(0); return -1; }
            );
            """;

        // Act.
        var result = Compiler.Compile(resultCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
    }
}
