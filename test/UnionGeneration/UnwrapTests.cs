using System.Reflection;

namespace Dunet.Test.UnionGeneration;

public sealed class UnwrapTests
{
    [Fact]
    public void CanUseUnwrapMethodToUnsafelyGetVariantValue()
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
        var compilation = Compiler.Compile(programCs);
        var value = compilation.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        compilation.CompilationErrors.Should().BeEmpty();
        compilation.GenerationErrors.Should().BeEmpty();
        value.Should().Be(1);
    }

    [Fact]
    public void CanUseUnwrapMethodToUnsafelyGetGenericVariantValue()
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
        var compilation = Compiler.Compile(programCs);
        var value = compilation.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        compilation.CompilationErrors.Should().BeEmpty();
        compilation.GenerationErrors.Should().BeEmpty();
        value.Should().Be(1);
    }

    [Fact]
    public void UnwrapMethodThrowsWhenCalledWithWrongUnderlyingValue()
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
        var compilation = Compiler.Compile(programCs);
        var action = () => compilation.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        compilation.CompilationErrors.Should().BeEmpty();
        compilation.GenerationErrors.Should().BeEmpty();
        action
            .Should()
            .Throw<TargetInvocationException>()
            .WithInnerExceptionExactly<InvalidOperationException>()
            .WithMessage("Called `Option.UnwrapSome()` on `None` value.");
    }
}
