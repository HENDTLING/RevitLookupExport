// Copyright (c) Lookup Foundation and Contributors
// 
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
// 
// THIS PROGRAM IS PROVIDED "AS IS" AND WITH ALL FAULTS.
// NO IMPLIED WARRANTY OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE IS PROVIDED.
// THERE IS NO GUARANTEE THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.

using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using Microsoft.Win32;
using RevitLookup.Abstractions.Services.Export;
using RevitLookup.Abstractions.Services.Presentation;

namespace RevitLookup.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class ExportProjectCsvCommand : ExternalCommand
{
    public override void Execute()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export Project to CSV",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            OverwritePrompt = true,
            FileName = "RevitProjectExport.csv"
        };

        if (dialog.ShowDialog() != true) return;

        var filePath = dialog.FileName;
        var exportService = Host.GetService<ICsvExportService>();
        var notifications = Host.GetService<INotificationService>();

        Task.Run(async () =>
        {
            try
            {
                await exportService.ExportProjectAsync(filePath);
                notifications.ShowSuccess("CSV Export", $"Exported to {filePath}");
            }
            catch (Exception exception)
            {
                notifications.ShowError("CSV Export failed", exception);
            }
        });
    }
}

