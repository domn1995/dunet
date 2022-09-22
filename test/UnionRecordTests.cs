using Dunet.Test.Compiler;
using Dunet.GenerateUnionRecord;

namespace Dunet.Test;

public abstract class UnionRecordTests
{
    public static Compile Compile { get; } = new(new UnionRecordGenerator());
}
