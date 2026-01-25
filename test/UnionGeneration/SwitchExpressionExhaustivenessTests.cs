namespace Dunet.Test.UnionGeneration;

public sealed class SwitchExpressionExhaustivenessTests
{
    private static readonly string unionDeclaration = """
        [Union]
        partial record Shape
        {
            partial record Circle(double Radius);
            partial record Rectangle(double Length, double Width);
            partial record Triangle(double Base, double Height);
        }
        """;

    [Fact]
    public async Task DoesNotWarnAboutExhaustiveSwitchExpression()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                Rectangle r => r.Length * r.Width,
                Circle c => 3.14 * c.Radius * c.Radius,
                Triangle t => t.Base * t.Height / 2,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task DefaultCaseOnlyIsExhaustive()
    {
        // Arrange.
        var source = $$"""
            using Dunet;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                _ => 0,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task WarnsOnMissingCase()
    {
        // Arrange.
        var source = $$"""
            using Dunet;            
            using static Shape;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                Circle c => 3.14 * c.Radius * c.Radius,
                Rectangle r => r.Length * r.Width,
                // Missing Triangle case.
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,19): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task WarnsOnMultipleMissingCases()
    {
        // Arrange.
        var source = $$"""
            using Dunet;            
            using static Shape;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                Circle c => 3.14 * c.Radius * c.Radius,
                // Missing Rectangle and Triangle cases.
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,19): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task WarnsWhenPatternMatchingIsNotExhaustive()
    {
        // Arrange.
        var source = $$"""
            using Dunet;            
            using static Shape;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                Rectangle r => r.Length * r.Width,
                Circle { Radius: > 0 } c => 3.14 * c.Radius * c.Radius,
                Triangle t => t.Base * t.Height / 2,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,19): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Circle{ Radius: 0D }' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnExhaustiveDeconstruction()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                Rectangle(var length, var width) => length * width,
                Circle(var radius) => 3.14 * radius * radius,
                Triangle(var @base, var height) t => @base * height / 2,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task WarnsOnNonExhaustiveDeconstruction()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape circle = new Shape.Circle(3.14);

            var area = circle switch
            {
                // Does not handle Rectangle with Length != 5.
                Rectangle(5, var width) => 5 * width,
                Circle(var radius) => 3.14 * radius * radius,
                Triangle(var @base, var height) t => @base * height / 2,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,19): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Rectangle(0D, _)' is not covered."
            );
    }

    [Fact]
    public async Task WarnsOnUnhandledNullCase()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape? circle = null;

            var sides = circle switch
            {
                Rectangle => 4,
                Circle => 0,
                Triangle t => 3,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(7,20): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnVarPattern()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape circle = new Shape.Circle(3.14);

            var sides = circle switch
            {
                Rectangle => 4,
                var shape => 0,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task VarPatternHandlesNullableUnionCase()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape? circle = null;

            var sides = circle switch
            {
                Rectangle => 4,
                Circle => 0,
                var shape => -1,
            };

            {{unionDeclaration}}
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
