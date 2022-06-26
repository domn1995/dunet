using System.Text;

namespace Dunet.GenerateUnionRecord;

internal static class UnionRecordSource
{
    public static string GenerateRecord(UnionRecord record)
    {
        var builder = new StringBuilder();

        foreach (var import in record.Imports)
        {
            builder.AppendLine(import);
        }

        if (record.Namespace is not null)
        {
            builder.AppendLine($"namespace {record.Namespace};");
        }

        builder.AppendLine($"abstract partial record {record.Name}");
        builder.AppendLine("{");
        builder.AppendLine($"    private {record.Name}() {{}}");

        foreach (var member in record.Members)
        {
            builder.AppendLine($"    public sealed partial record {member.Name} : {record.Name};");
            builder.AppendLine();
        }

        builder.AppendLine("}");

        return builder.ToString();
    }
}
