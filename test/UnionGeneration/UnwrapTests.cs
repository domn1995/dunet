using System.Reflection;

namespace Dunet.Test.UnionGeneration;

public sealed class UnwrapTests
{
    [Fact]
    public async Task CanUseUnwrapMethodToUnsafelyGetVariantValue()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            var value = GetValue();

            static int GetValue()
            {
                var option = new Option.Some(1);
                return option.UnwrapSome().Value;
            }

            [Union]
            public partial record Option
            {
                public partial record Some(int Value);
                public partial record None;
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);
        var value = result.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        value.Should().Be(1);
    }

    [Fact]
    public async Task CanUseUnwrapMethodToUnsafelyGetGenericVariantValue()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            var value = GetValue();

            static int GetValue()
            {
                var option = new Option<int>.Some(1);
                return option.UnwrapSome().Value;
            }

            [Union]
            public partial record Option<T>
            {
                public partial record Some(T Value);
                public partial record None;
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);
        var value = result.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        value.Should().Be(1);
    }

    [Fact]
    public async Task UnwrapMethodThrowsWhenCalledWithWrongUnderlyingValue()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            var value = GetValue();

            static int GetValue()
            {
                var option = new Option.None();
                return option.UnwrapSome().Value;
            }

            [Union]
            public partial record Option
            {
                public partial record Some(int Value);
                public partial record None;
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(programCs);
        var action = () => result.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
        action
            .Should()
            .Throw<TargetInvocationException>()
            .WithInnerExceptionExactly<InvalidOperationException>()
            .WithMessage("Called `Option.UnwrapSome()` on `None` value.");
    }
}
