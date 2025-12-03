using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BarcodeApp.Models;

namespace BarcodeApp.Services;

public static class BatchExportService
{
    public static void ExportToCsv(IEnumerable<BarcodeItem> items, string filePath, bool includeHeader = true)
    {
        var lines = new List<string>();
        
        if (includeHeader)
        {
            lines.Add("Data,Symbology,Quantity,ModuleWidth,BarcodeHeight,ShowText");
        }
        
        foreach (var item in items)
        {
            var moduleWidth = item.ModuleWidth?.ToString() ?? string.Empty;
            var height = item.BarcodeHeight?.ToString() ?? string.Empty;
            var showText = item.ShowText?.ToString() ?? string.Empty;
            
            // Escape commas and quotes in data
            var escapedData = EscapeCsvField(item.Data);
            
            lines.Add($"{escapedData},{item.Symbology},{item.Quantity},{moduleWidth},{height},{showText}");
        }
        
        File.WriteAllLines(filePath, lines);
    }
    
    public static void ExportToText(IEnumerable<BarcodeItem> items, string filePath)
    {
        var lines = items.Select(item => item.Data);
        File.WriteAllLines(filePath, lines);
    }
    
    public static void ExportValidationResults(IEnumerable<BatchItemValidationResult> results, string filePath)
    {
        var lines = new List<string>
        {
            "Index,Data,Symbology,Quantity,IsValid,ErrorMessage"
        };
        
        foreach (var result in results)
        {
            var escapedData = EscapeCsvField(result.Item.Data);
            var escapedError = EscapeCsvField(result.ErrorMessage);
            
            lines.Add($"{result.Index},{escapedData},{result.Item.Symbology},{result.Item.Quantity},{result.IsValid},{escapedError}");
        }
        
        File.WriteAllLines(filePath, lines);
    }
    
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;
        
        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        
        return field;
    }
}

