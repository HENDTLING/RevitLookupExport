using System.Threading.Tasks;

namespace RevitLookup.Abstractions.Services.Export;

public interface ICsvExportService
{
    /// <summary>
    ///     Exports the entire active project database to a CSV file at the given path.
    /// </summary>
    /// <param name="filePath">Absolute path to the CSV file to write.</param>
    Task ExportProjectAsync(string filePath);
}

