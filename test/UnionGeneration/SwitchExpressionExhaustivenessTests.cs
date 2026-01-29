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

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
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
    public async Task DoesNotWarnAboutMultipleExhaustiveSwitchExpressions()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle r => r.Length * r.Width,
                Circle c => 3.14 * c.Radius * c.Radius,
                Triangle t => t.Base * t.Height / 2,
            };

            var area2 = shape switch
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

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
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

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task WarnsOnMultipleMissingCases()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task WarnsWhenPatternMatchingIsNotExhaustive()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Circle{ Radius: 0D }' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnExhaustiveDeconstruction()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
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

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Rectangle(0D, _)' is not covered."
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

            Shape? shape = null;

            var sides = shape switch
            {
                Rectangle => 4,
                Circle => 0,
                Triangle => 3,
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
                == "(7,19): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task NoWarningWhenExplicitlyHandlingNullCase()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape? shape = null;

            var sides = shape switch
            {
                Rectangle => 4,
                Circle => 0,
                Triangle t => 3,
                null => 0,
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
    public async Task DoesNotWarnOnVarPattern()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var sides = shape switch
            {
                Rectangle => 4,
                var s => 0,
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

            Shape? shape = null;

            var sides = shape switch
            {
                Rectangle => 4,
                Circle => 0,
                var s => -1,
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
    public async Task WarnsOnRecursivePatternWithMultiplePropertyPatterns()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle { Length: > 0, Width: > 0 } r => r.Length * r.Width,
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
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Rectangle{ Length: 0D }' is not covered."
            );
    }

    [Fact]
    public async Task WarnsWhenGuardMakesPatternNonExhaustive()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle r when r.Length > 0 => r.Length * r.Width,
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
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task WarnsOnRecursivePatternWithBothPositionalAndPropertyClauses()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(var length, var width) { Length: > 0 } r => length * width,
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
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Rectangle(_, _) { Length: 0D }' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnExhaustiveMixedPositionalAndSimplePatterns()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(var length, var width) => length * width,
                Circle => 0,
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
    public async Task DoesNotWarnOnExhaustiveDeconstructionWithMixedStyles()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(var length, var width) => length * width,
                Circle(var radius) => 3.14 * radius * radius,
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
    public async Task WarnsOnConstantValuesInPositionalPatterns()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(5, 10) => 50,
                Circle(var radius) => 3.14 * radius * radius,
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Rectangle(0D, _)' is not covered."
            );
    }

    [Fact]
    public async Task WarnsOnConstantValueInSecondPositionalParameter()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(var length, 0) => 0,
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
        result
            .Warnings.Should()
            .OnlyContain(static diagnostic =>
                diagnostic.ToString()
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Rectangle(_, 5E-324D)' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnPropertyPatternWithVar()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(var length, var width) => length * width,
                Circle { Radius: var radius } => 3.14 * radius * radius,
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
    public async Task DoesNotWarnOnPropertyPatternWithMultipleVarSubpatterns()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Rectangle(10, 5);

            var area = shape switch
            {
                Rectangle { Length: var length, Width: var width } => length * width,
                Circle(var radius) => 3.14 * radius * radius,
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
    public async Task DoesNotWarnOnPropertyPatternWithDiscard()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(var length, var width) => length * width,
                Circle { Radius: _ } => 0,
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
    public async Task WarnsOnPropertyPatternWithConstrainedValue()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle(var length, var width) => length * width,
                Circle { Radius: > 0 } => 1,
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Circle{ Radius: 0D }' is not covered."
            );
    }

    [Fact]
    public async Task WarnsOnPropertyPatternWithConstrainedValueAndVarPattern()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Rectangle { Length: var length, Width: > 0 } => length * length,
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern 'Shape.Rectangle{ Length: _,  Width: 0D }' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnExhaustiveOrPattern()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Circle or Rectangle => 0,
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
    public async Task DoesNotWarnOnExhaustiveOrPatternWithAllVariants()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Circle or Rectangle or Triangle => 0,
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
    public async Task WarnsOnNonExhaustiveOrPattern()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Circle or Rectangle => 0,
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
                == "(6,18): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnOrPatternMixedWithOtherPatterns()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Circle or Triangle => 0,
                Rectangle(var length, var width) => length * width,
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
    public async Task DoesNotWarnOnOrPatternWithDeconstruction()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Circle(_) or Triangle(_, _) => 1,
                Rectangle(var length, var width) => length * width,
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
    public async Task DoesNotWarnOnOrPatternWithDeclarationPatterns()
    {
        // Arrange.
        var source = $$"""
            using Dunet;
            using static Shape;

            Shape shape = new Shape.Circle(3.14);

            var area = shape switch
            {
                Circle or Rectangle => 1,
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
    public async Task HandlesOrPatternWithNullableUnion()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape? shape = null;

            var sides = shape switch
            {
                Rectangle or Triangle => 1,
                Circle => 0,
                null => -1,
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
    public async Task WarnsOnOrPatternNotHandlingNull()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape? shape = null;

            var sides = shape switch
            {
                Circle or Rectangle or Triangle => 0,
                // Missing null case.
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
                == "(7,19): warning CS8509: The switch expression does not handle all possible values of its input type (it is not exhaustive). For example, the pattern '_' is not covered."
            );
    }

    [Fact]
    public async Task DoesNotWarnOnOrPatternWithUnionVariantAndNull()
    {
        // Arrange.
        var source = $$"""
            using System;
            using Dunet;
            using static Shape;

            Shape? shape = null;

            var sides = shape switch
            {
                Circle or null => 0,
                Rectangle => 1,
                Triangle => 2,
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
