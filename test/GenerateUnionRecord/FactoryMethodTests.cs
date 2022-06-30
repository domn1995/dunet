using System.Reflection;
using Dunet.Test.Compiler;

namespace Dunet.Test.GenerateUnionRecord;

public class FactoryMethodTests : UnionRecordTests
{
    [Fact]
    public void FactoryMethodsAreNotGeneratedByDefault()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

var union = new ComplexUnion<string>.Argless();

[Union]
public partial record ComplexUnion<T>
{
    public partial record GenericCase(T Generic, string Test);
    public partial record Argless();
    public partial record RecursiveCase(ComplexUnion<T> Inner);
}";

        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();

        GetStaticMethodNames(result, "ComplexUnion`1")
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void FactoryMethodGenerationCanBeCustomised()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

ComplexUnion<int> generic = ComplexUnion<int>.MakeGenericCaseNow(15, ""Hello"");
ComplexUnion<int> argless = ComplexUnion<int>.MakeArglessNow();
ComplexUnion<int> recursive = ComplexUnion<int>.MakeRecursiveCaseNow(generic);

[Union(GenerateFactoryMethods = true, FactoryMethodPrefix = ""Make"", FactoryMethodSuffix = ""Now"")]
partial record ComplexUnion<T>
{
    partial record GenericCase(T Generic, string Test);
    partial record Argless();
    partial record RecursiveCase(ComplexUnion<T> Inner);
}";

        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();

        GetStaticMethodNames(result, "ComplexUnion`1")
            .Should()
            .BeEquivalentTo("MakeGenericCaseNow", "MakeArglessNow", "MakeRecursiveCaseNow");
    }

    [Fact]
    public void FactoryMethodGenerationCanBeDisabled()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

var union = new ComplexUnion<string>.Argless();

[Union(GenerateFactoryMethods = false)]
partial record ComplexUnion<T>
{
    partial record GenericCase(T Generic, string Test);
    partial record Argless();
    partial record RecursiveCase(ComplexUnion<T> Inner);
}";

        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();

        GetStaticMethodNames(result, "ComplexUnion`1")
            .Should()
            .BeEmpty();
    }

    private static IEnumerable<string> GetStaticMethodNames(CompilationResult result, string typeName)
    {
        result.Assembly.Should().NotBeNull();
        var unionType = result.Assembly.GetType(typeName);
        return unionType.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name);
    }
}
