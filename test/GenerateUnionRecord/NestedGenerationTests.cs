namespace Dunet.Test.GenerateUnionRecord;

/// <summary>
/// Tests the unions are properly generated when their definitions are nested within other classes.
/// </summary>
public class NestedGenerationTests : UnionRecordTests
{
    [Fact]
    public void CanReturnNestedMember()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

var foo = Parent.Foo();
var bar = Parent.Bar();

public partial class Parent
{
    [Union]
    public partial record Nested
    {
        public partial record Member1;
        public partial record Member2;
    }

    public static Nested Foo()
    {
        return new Nested.Member1();
    }

    public static Nested Bar()
    {
        return new Nested.Member2();
    }
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void CanReturnDeeplyNestedMember()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

var foo = Parent1.Parent2.Parent3.Foo();
var bar = Parent1.Parent2.Parent3.Bar();

public partial class Parent1
{
    public partial class Parent2
    {
        public partial class Parent3
        {
            [Union]
            public partial record Nested
            {
                public partial record Member1;
                public partial record Member2;
            }

            public static Nested Foo()
            {
                return new Nested.Member1();
            }

            public static Nested Bar()
            {
                return new Nested.Member2();
            }
        }
    }
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
