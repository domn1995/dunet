using Dunet.Test.Compiler;
using Dunet.UnionRecord;

namespace Dunet.Test.UnionRecord;

public abstract class UnionRecordTests
{
    public static Compile Compile { get; } = new(new UnionRecordGenerator());
}
