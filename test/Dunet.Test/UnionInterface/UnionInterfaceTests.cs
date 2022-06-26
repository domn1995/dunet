using Dunet.Test.Compiler;

namespace Dunet.Test.UnionInterface;

public abstract class UnionInterfaceTests
{
    protected static Compile Compile { get; } = new(new UnionInterfaceGenerator());
}
