using System.Reflection;

namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class UnwrapTests
{
    [Fact]
    public void CanUseUnwrapMethodToUnsafelyGetVariantValue()
    {
        // Arrange.
        var unionCs = """
using Dunet;

namespace Options;

[Union]
public partial record Option
{
    public partial record Some(int Value);
    public partial record None;
}
""";

        var programCs = """
using Options;

var value = GetValue();

static int GetValue()
{
    var option = new Option.Some(1);
    return option.UnwrapSome().Value;
}
""";

        // Act.
        var result = Compiler.Compile(unionCs, programCs);
        var value = result.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(1);
    }

    [Fact]
    public void CanUseUnwrapMethodToUnsafelyGetGenericVariantValue()
    {
        // Arrange.
        var unionCs = """
using Dunet;

namespace Options;

[Union]
public partial record Option<T>
{
    public partial record Some(T Value);
    public partial record None;
}
""";

        var programCs = """
using Options;

var value = GetValue();

static int GetValue()
{
    var option = new Option<int>.Some(1);
    return option.UnwrapSome().Value;
}
""";

        // Act.
        var result = Compiler.Compile(unionCs, programCs);
        var value = result.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(1);
    }

    [Fact]
    public void UnwrapMethodThrowsWhenCalledWithWrongUnderlyingValue()
    {
        // Arrange.
        var unionCs = """
using Dunet;

namespace Options;

[Union]
public partial record Option
{
    public partial record Some(int Value);
    public partial record None;
}
""";

        var programCs = """
using Options;

var value = GetValue();

static int GetValue()
{
    var option = new Option.None();
    return option.UnwrapSome().Value;
}
""";

        // Act.
        var result = Compiler.Compile(unionCs, programCs);
        var action = () => result.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        action
            .Should()
            .Throw<TargetInvocationException>()
            .WithInnerExceptionExactly<InvalidOperationException>()
            .WithMessage("Called `Option.UnwrapSome()` on `None` value.");
    }
}
