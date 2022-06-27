namespace Dunet.Test.GenerateUnionRecord;

public class FactoryMethodTests : UnionRecordTests
{
    [Fact]
    public void FactoryMethodsAreGeneratedByDefault()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

ComplexUnion<int> generic = ComplexUnion<int>.NewGenericCase(15, ""Hello"");
ComplexUnion<int> argless = ComplexUnion<int>.NewArgless();
ComplexUnion<int> recursive = ComplexUnion<int>.NewRecursiveCase(generic);

[Union]
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

[Union(FactoryMethodPrefix = ""Make"", FactoryMethodSuffix = ""Now"")]
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
    }
    
    [Fact]
    public void FactoryMethodGenerationCanBeDisabled()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

ComplexUnion<int> generic = ComplexUnion<int>.NewGenericCase(15, ""Hello"");
ComplexUnion<int> argless = ComplexUnion<int>.NewArgless();
ComplexUnion<int> recursive = ComplexUnion<int>.NewRecursiveCase(generic);

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
        result.CompilationErrors.Should().HaveCount(3);
        result.CompilationErrors.Should().AllSatisfy(diagnostic => diagnostic.Id.Should().Be("CS0117"));
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
