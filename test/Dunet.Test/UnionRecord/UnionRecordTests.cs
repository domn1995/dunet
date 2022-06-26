using Dunet.Test.Compiler;
using Dunet.GenerateUnionRecord;

namespace Dunet.Test.UnionRecord;

public abstract class UnionRecordTests
{
    public static Compile Compile { get; } = new(new UnionRecordGenerator());
}
