namespace Dunet.Test.GenerateUnionRecord;

public class GenerationTests : UnionRecordTests
{
    [Fact]
    public void UnionMembersExtendUnionType()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

QueryState loading = new QueryState.Loading();
QueryState loadingFactory = QueryState.NewLoading();
QueryState success = new QueryState.Success();
QueryState successFactory = QueryState.NewSuccess();
QueryState error = new QueryState.Error();
QueryState errorFactory = QueryState.NewError();

[Union(GenerateFactoryMethods = true)]
partial record QueryState
{
    partial record Loading();
    partial record Success();
    partial record Error();
}";

        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionTypeMayHaveNoMembers()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;

var loading = new QueryState.Loading();
var success = new QueryState.Success();
var error = new QueryState.Error();

[Union]
partial record QueryState
{
    partial record Loading();
    partial record Success();
    partial record Error();
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
        // Arrange.
        var programCs =
            @"
using Dunet;

var single = new Single.OnlyMember();

[Union]
partial record Single
{
    partial record OnlyMember();
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
        // Arrange.
        var programCs =
            @"
using Dunet;

// Must have something for top level program to compile.
var dummy = 1;

[Union]
partial record Empty;";
        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().BeEmpty();
        result.GenerationDiagnostics.Should().BeEmpty();
    }

    [Fact]
    public void UnionTypeMayHaveComplexMembers()
    {
        // Arrange.
        var programCs =
            @"
using Dunet;
using System;

var success = new Result.Success(Guid.NewGuid());
var failure = new Result.Failure(new Exception(""Boom!""));

[Union]
partial record Result
{
    partial record Success(Guid Id);
    partial record Failure(Exception Error);
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
        // Arrange.
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
partial record Result
{
    partial record Success(Data Value);
    partial record Failure(Exception Error);
}";

        var programCs =
            @"
using System;
using ComplexTypes;
using Results;

// Must have something for top level program to execute.
var success = new Result.Success(new Data(""foo""));
var failure = new Result.Failure(new Exception(""bar""));
";
        // Act.
        var result = Compile.ToAssembly(dataCs, iResultCs, programCs);

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

QueryState loading = QueryState.NewLoading();

[Union(GenerateFactoryMethods = false)]
partial record QueryState
{
    partial record Loading();
    partial record Success();
    partial record Error();
}";

        // Act.
        var result = Compile.ToAssembly(programCs);

        // Assert.
        result.CompilationErrors.Should().HaveCount(1);
        result.CompilationErrors[0].Id.Should().Be("CS0117"); // error CS0117: 'QueryState' does not contain a definition for 'NewLoading'
        result.GenerationDiagnostics.Should().BeEmpty();
    }
}
