namespace Dunet.Test.UnionGeneration;

public sealed class NamespaceTests
{
    [Fact]
    public void CanReferenceUnionTypesFromSeparateNamespace()
    {
        // Arrange.
        var iShapeCs = """
            using Dunet;

            namespace Shapes;

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }
            """;

        var programCs = """
            using System;
            using Shapes;

            namespace Test;

            public static class Program
            {
                public static void Main()
                {
                    Shape circle = new Shape.Circle(3.14);
                    Shape rectangle = new Shape.Rectangle(1.5, 3.5);
                    Shape triangle = new Shape.Triangle(2.0, 3.0);
                }
            }
            """;

        // Act.
        var result = Compiler.Compile(iShapeCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void CanUseUnionTypesInSameNamespace()
    {
        var programCs = """
            using Dunet;

            namespace Test;

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }

            public static class Program
            {
                public static void Main()
                {
                    Shape circle = new Shape.Circle(3.14);
                    Shape rectangle = new Shape.Rectangle(1.5, 3.5);
                    Shape triangle = new Shape.Triangle(2.0, 3.0);
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
    public void CanUseUnionTypesInTopLevelPrograms()
    {
        var programCs = """
            using Dunet;

            Shape circle = new Shape.Circle(3.14);
            Shape rectangle = new Shape.Rectangle(1.5, 3.5);
            Shape triangle = new Shape.Triangle(2.0, 3.0);

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
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
    public void CanUseUnionTypesWithBlockScopedNamespaces()
    {
        var programCs = """
            using Dunet;

            namespace Test
            {
                [Union]
                partial record Shape
                {
                    partial record Circle(double Radius);
                    partial record Rectangle(double Length, double Width);
                    partial record Triangle(double Base, double Height);
                }

                public static class Program
                {
                    public static void Main()
                    {
                        Shape circle = new Shape.Circle(3.14);
                        Shape rectangle = new Shape.Rectangle(1.5, 3.5);
                        Shape triangle = new Shape.Triangle(2.0, 3.0);
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
    public void CanUseUnionTypesWithFileScopedNamespaces()
    {
        var programCs = """
            using Dunet;

            namespace Test;

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }

            public static class Program
            {
                public static void Main()
                {
                    Shape circle = new Shape.Circle(3.14);
                    Shape rectangle = new Shape.Rectangle(1.5, 3.5);
                    Shape triangle = new Shape.Triangle(2.0, 3.0);
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
    public void CanReferenceUnionTypesFromSeparateFileScopedNamespace()
    {
        // Arrange.
        var iShapeCs = """
            using Dunet;

            namespace Shapes;

            [Union]
            partial record Shape
            {
                partial record Circle(double Radius);
                partial record Rectangle(double Length, double Width);
                partial record Triangle(double Base, double Height);
            }
            """;

        var programCs = """
            using System;
            using Shapes;

            namespace Test;

            public static class Program
            {
                public static void Main()
                {
                    Shape circle = new Shape.Circle(3.14);
                    Shape rectangle = new Shape.Rectangle(1.5, 3.5);
                    Shape triangle = new Shape.Triangle(2.0, 3.0);
                }
            }
            """;

        // Act.
        var result = Compiler.Compile(iShapeCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void CanReferenceUnionTypesFromSeparateBlockScopedNamespace()
    {
        // Arrange.
        var iShapeCs = """
            using Dunet;

            namespace Shapes
            {
                [Union]
                partial record Shape
                {
                    partial record Circle(double Radius);
                    partial record Rectangle(double Length, double Width);
                    partial record Triangle(double Base, double Height);
                }
            }
            """;

        var programCs = """
            using System;
            using Shapes;

            namespace Test;

            public static class Program
            {
                public static void Main()
                {
                    Shape circle = new Shape.Circle(3.14);
                    Shape rectangle = new Shape.Rectangle(1.5, 3.5);
                    Shape triangle = new Shape.Triangle(2.0, 3.0);
                }
            }
            """;

        // Act.
        var result = Compiler.Compile(iShapeCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void CanHaveMultipleUnionsWithSameNameInSeparateNamespaces()
    {
        // Arrange.
        var resultCs = """
            using Dunet;

            namespace Foo;

            [Union]
            partial record Result
            {
                partial record Success();
                partial record Failure();
            }
            """;

        var otherResultCs = """
            using Dunet;

            namespace Bar;

            [Union]
            partial record Result
            {
                partial record Ok();
                partial record Error();
            }
            """;

        var programCs = """
            Foo.Result success = new Foo.Result.Success();
            Foo.Result failure = new Foo.Result.Failure();
            Bar.Result ok = new Bar.Result.Ok();
            Bar.Result error = new Bar.Result.Error();
            """;

        // Act.
        var result = Compiler.Compile(resultCs, otherResultCs, programCs);

        // Assert.
        using var scope = new AssertionScope();
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
