namespace Dunet.Test.UnionGeneration;

public sealed class ImplicitConversionTests
{
    [Fact]
    public async Task UnionVariantIsAssignableToUnionType()
    {
        var programCs = """
            using Dunet;
            using System;

            Result success = "Hello, world!";
            Result error = new Exception("Boom!");

            [Union]
            partial record Result
            {
                partial record Success(string Value);
                partial record Failure(Exception Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task UnionVariantPropertyTypeSubclassIsAssignableToUnionType()
    {
        var programCs = """
            using Dunet;
            using System;
            using System.Collections.Generic;

            Result success = new StringList() { "foo", "bar", "baz" };
            Result error = new InvalidOperationException("Boom!");

            class StringList : List<string> {}

            [Union]
            partial record Result
            {
                partial record Success(List<string> Values);
                partial record Failure(Exception Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task UnionVariantGenericPropertyIsAssignableToUnionType()
    {
        var programCs = """
            using Dunet;
            using System;

            Result<int> success = 42;
            Result<string> error = new Exception("Something went wrong.");

            [Union]
            partial record Result<T>
            {
                partial record Success(T Value);
                partial record Failure(Exception Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task VariantOfMultiGenericUnionIsAssignableToUnionType()
    {
        var programCs = """
            using Dunet;
            using System;
            using static Result<string, int>;

            Result<string, int> success = 42;
            Result<string, int> error = "Something went wrong.";

            [Union]
            partial record Result<TFailure, TSuccess>
            {
                partial record Success(TSuccess Value);
                partial record Failure(TFailure Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task ImplicitConversionIsNotCreatedForVariantWithInterfaceProperty()
    {
        var programCs = """
            using Dunet;
            using System;
            using System.Collections.Generic;
            using static Result<int>;

            Result<int> success = new Success(42);
            Result<int> error = new Failure(new string[] { "Error 1", "Error 2", "Error 3" });

            [Union]
            partial record Result<T>
            {
                partial record Success(T Value);
                partial record Failure(IEnumerable<string> Errors);
            }
            """;

        // Act.
        using var scope = new AssertionScope();
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task IgnoresEmptyMembersWhenGeneratingImplicitConversions()
    {
        var programCs = """
            using Dunet;
            using System;
            using static Option<int>;

            Option<int> success = 42;
            Option<int> none = new None();

            [Union]
            partial record Option<T>
            {
                partial record Some(T Value);
                partial record None();
            }
            """;

        // Act.
        using var scope = new AssertionScope();
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task UnionWithEnableImplicitConversionsTrueGeneratesOperators()
    {
        var programCs = """
            using Dunet;

            Result success = 42;
            Result error = "error";

            [Union(EnableImplicitConversions = true)]
            partial record Result
            {
                partial record Success(int Value);
                partial record Failure(string Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task UnionWithEnableImplicitConversionsFalseSkipsOperators()
    {
        var programCs = """
            using Dunet;

            Result success = new Result.Success(42);
            Result error = new Result.Failure("error");

            [Union(EnableImplicitConversions = false)]
            partial record Result
            {
                partial record Success(int Value);
                partial record Failure(string Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task UnionWithEnableImplicitConversionsFalseCannotUseImplicitOperators()
    {
        var programCs = """
            using Dunet;

            Result success = 42;

            [Union(EnableImplicitConversions = false)]
            partial record Result
            {
                partial record Success(int Value);
                partial record Failure(string Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UnionWithEnableImplicitConversionsFalseRequiresExplicitConstruction()
    {
        var programCs = """
            using Dunet;

            Result success = new Result.Success(42);
            Result error = new Result.Failure("error");

            [Union(EnableImplicitConversions = false)]
            partial record Result
            {
                partial record Success(int Value);
                partial record Failure(string Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task GenericUnionWithEnableImplicitConversionsFalseSkipsOperators()
    {
        var programCs = """
            using Dunet;

            Result<int> success = new Result<int>.Success(42);
            Result<string> error = new Result<string>.Failure("error");

            [Union(EnableImplicitConversions = false)]
            partial record Result<T>
            {
                partial record Success(T Value);
                partial record Failure(string Error);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task UnionWithEnableImplicitConversionsFalseAndEmptyVariantSkipsOperators()
    {
        var programCs = """
            using Dunet;

            Option<int> some = new Option<int>.Some(42);
            Option<int> none = new Option<int>.None();

            [Union(EnableImplicitConversions = false)]
            partial record Option<T>
            {
                partial record Some(T Value);
                partial record None();
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task MultipleUnionsSomeWithImplicitConversionsDisabled()
    {
        var programCs = """
            using Dunet;

            Result<int> success = 42;
            Response<int> response = new Response<int>.Ok(100);

            [Union]
            partial record Result<T>
            {
                partial record Success(T Value);
                partial record Failure(string Error);
            }

            [Union(EnableImplicitConversions = false)]
            partial record Response<T>
            {
                partial record Ok(T Value);
                partial record Error(string Message);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
