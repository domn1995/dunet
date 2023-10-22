namespace Dunet.Test.UnionExtensionsGeneration;

public sealed class UnwrapTests
{
    [Fact]
    public void CanUseToVariantMethodToUnsafelyUnwrapVariant()
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
    Option option = new Option.Some(1);
    return option.ToSome().Value;
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
    public void CanUseAsVariantMethodToSafelyUnwrapVariant()
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

static int? GetValue()
{
    Option option = new Option.Some(1);
    return option.AsSome()?.Value;
}
""";

        // Act.
        var result = Compiler.Compile(unionCs, programCs);
        var value = result.Assembly?.ExecuteStaticMethod<int?>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(1);
    }

    [Fact]
    public void CanUseTryVariantMethodToSafelyUnwrapVariant()
    {
        // Arrange.
        var source = """
using Dunet;

static int GetValue() => option.TrySome(out var result) ? result : -1;

[Union]
partial record Option
{
    partial record Some(int Value);
    partial record None;
}
""";

        // Act.
        var result = Compiler.Compile(source);
        var value = result.Assembly?.ExecuteStaticMethod<int>("GetValue");

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationErrors.Should().BeEmpty();
        value.Should().Be(1);
    }
}
