using Dunet.Test.Compiler;

namespace Dunet.Test;

public class UnionGenerationTests
{
    [Fact]
    public void UnionTypeMayHaveNoMembers()
    {
        var programCs =
            @"
using Dunet;

IQueryState loading = new Loading();
IQueryState success = new Success();
IQueryState error = new Error();

[Union]
interface IQueryState
{
    IQueryState Loading();
    IQueryState Success();
    IQueryState Error();
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionMayContainSingleType()
    {
        var programCs =
            @"
using Dunet;

ISingle single = new Single();

[Union]
interface ISingle
{
    ISingle Single();
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionMayBeEmpty()
    {
        var programCs =
            @"
using Dunet;

// Must have something for top level program to execute.
var dummy = 1;

[Union]
interface IEmpty { }";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
