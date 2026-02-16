namespace Dunet.Test.UnionGeneration;

public sealed class NullableReferenceTypes
{
    [Fact]
    public async Task AllowsNullableVariantParameters()
    {
        // Arrange.
        var source = $$"""
            using Dunet;

            Message message = new Message.NullableStr(null);

            _ = message switch
            {
                Message.Nothing => "Nothing",
                Message.Str(var value) => value,
                Message.NullableStr(var value) => value ?? "",
            };


            [Union]
            public partial record Message
            {
                public partial record Nothing;
                public partial record Str(string Value);
                public partial record NullableStr(string? Value);
            }
            """;

        // Act.
        var result = await Compiler.CompileAsync(source);

        // Assert.
        using var scope = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
