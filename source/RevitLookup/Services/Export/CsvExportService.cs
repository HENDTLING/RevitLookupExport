using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using RevitLookup.Abstractions.Models.Decomposition;
using RevitLookup.Abstractions.ObservableModels.Decomposition;
using RevitLookup.Abstractions.Services.Decomposition;
using RevitLookup.Abstractions.Services.Export;
using RevitLookup.Core;
using RevitLookup.Core.Decomposition;

namespace RevitLookup.Services.Export;

public sealed class CsvExportService(
    IDecompositionService decompositionService,
    ILogger<CsvExportService> logger)
    : ICsvExportService
{
    public async Task ExportProjectAsync(string filePath)
    {
        var objects = await RevitShell.AsyncCollectionHandler.RaiseAsync(_ => RevitObjectsCollector.GetObjects(KnownDecompositionObject.Database));
        var decomposedObjects = await decompositionService.DecomposeAsync(objects);

        var csv = new StringBuilder(4096);
        csv.AppendLine("ObjectName,ObjectType,MemberDepth,MemberName,DeclaringType,ValueName,ValueType,ValueDescription");

        foreach (var decomposedObject in decomposedObjects)
        {
            var members = await decompositionService.DecomposeMembersAsync(decomposedObject);
            foreach (var member in members)
            {
                AppendCsvRow(csv, decomposedObject, member);
            }
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        logger.LogInformation("CSV export completed: {FilePath}", filePath);
    }

    private static void AppendCsvRow(StringBuilder csv, ObservableDecomposedObject obj, ObservableDecomposedMember member)
    {
        var fields = new[]
        {
            obj.Name,
            obj.TypeFullName,
            member.Depth.ToString(CultureInfo.InvariantCulture),
            member.Name,
            member.DeclaringTypeFullName,
            member.Value.Name,
            member.Value.TypeFullName,
            member.Value.Description ?? string.Empty
        };

        for (var i = 0; i < fields.Length; i++)
        {
            if (i > 0) csv.Append(',');
            csv.Append(EscapeCsv(fields[i] ?? string.Empty));
        }
        csv.AppendLine();
    }

    private static string EscapeCsv(string value)
    {
        if (value.IndexOfAny([',', '"', '\n', '\r']) == -1) return value;
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}

