namespace Dunet.Test.UnionInterface;

public class GenerationTests : UnionInterfaceTests
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

    [Fact]
    public void UnionTypeMayHaveComplexMembers()
    {
        var programCs =
            @"
using Dunet;
using System;

// Must have something for top level program to execute.
var dummy = 1;

[Union]
interface IResult
{
    void Success(Guid id);
    void Failure(Exception error);
}";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionTypeMayHaveComplexMembersFromOtherNamespace()
    {
        var dataCs =
            @"
namespace ComplexTypes;

public record Data(string Value);
";

        var iResultCs =
            @"
using Dunet;
using ComplexTypes;
using System;

namespace Results;

[Union]
public interface IResult
{
    void Success(Data value);
    void Failure(Exception error);
}";

        var programCs =
            @"
using System;
using ComplexTypes;
using Results;

// Must have something for top level program to execute.
var success = new Success(new Data(""foo""));
var failure = new Failure(new Exception(""foo""));
";
        // Act.
        var result = Compile.ToAssembly(dataCs, iResultCs, programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
