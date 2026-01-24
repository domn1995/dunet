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
}
