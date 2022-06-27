using Dunet.GenerateUnionInterface;
using Dunet.Test.Compiler;

namespace Dunet.Test.GenerateUnionInterface;

public abstract class UnionInterfaceTests
{
    protected static Compile Compile { get; } = new(new UnionInterfaceGenerator());
}
