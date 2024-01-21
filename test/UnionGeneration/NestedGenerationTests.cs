namespace Dunet.Test.UnionGeneration;

/// <summary>
/// Tests the unions are properly generated when their definitions are nested within other classes.
/// </summary>
public sealed class NestedGenerationTests
{
    [Fact]
    public void CanReturnNestedVariant()
    {
        // Arrange.
        var programCs = """
            using Dunet;

            var foo = Parent.Foo();
            var bar = Parent.Bar();

            public partial class Parent
            {
                [Union]
                public partial record Nested
                {
                    public partial record Variant1;
                    public partial record Variant2;
                }

                public static Nested Foo()
                {
                    return new Nested.Variant1();
                }

                public static Nested Bar()
                {
                    return new Nested.Variant2();
                }
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
    public void CanReturnDeeplyNestedVariant()
    {
        // Arrange.
        var programCs = """
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
                            public partial record Variant1;
                            public partial record Variant2;
                        }

                        public static Nested Foo()
                        {
                            return new Nested.Variant1();
                        }

                        public static Nested Bar()
                        {
                            return new Nested.Variant2();
                        }
                    }
                }
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
    public void CanReturnDeeplyNestedVariantFromOtherNamespace()
    {
        // Arrange.
        var nestedCs = """
            using Dunet;

            namespace NestedTests;

            public partial class Parent1
            {
                public partial class Parent2
                {
                    public partial class Parent3
                    {
                        [Union]
                        public partial record Nested
                        {
                            public partial record Variant1;
                            public partial record Variant2;
                        }

                        public static Nested Foo()
                        {
                            return new Nested.Variant1();
                        }

                        public static Nested Bar()
                        {
                            return new Nested.Variant2();
                        }
                    }
                }
            }
            """;

        var programCs = """
            using NestedTests;

            var foo = Parent1.Parent2.Parent3.Foo();
            var bar = Parent1.Parent2.Parent3.Bar();
            """;

        // Act.
        var result = Compiler.Compile(nestedCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void CanReturnMultipleDeeplyNestedUnionVariants()
    {
        // Arrange.
        var programCs = """
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
                        public partial record Nested1
                        {
                            public partial record Variant1;
                            public partial record Variant2;
                        }

                        [Union]
                        public partial record Nested2
                        {
                            public partial record Variant1;
                            public partial record Variant2;
                        }

                        public static Nested1 Foo()
                        {
                            return new Nested1.Variant1();
                        }

                        public static Nested2 Bar()
                        {
                            return new Nested2.Variant1();
                        }
                    }
                }
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
