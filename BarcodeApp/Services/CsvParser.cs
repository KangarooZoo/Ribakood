using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BarcodeApp.Models;

namespace BarcodeApp.Services;

public static class CsvParser
{
    public static List<BarcodeItem> ParseCsvFile(string filePath, BarcodeSymbology defaultSymbology = BarcodeSymbology.Code128)
    {
        var lines = File.ReadAllLines(filePath);
        if (lines.Length == 0)
        {
            return new List<BarcodeItem>();
        }

        // Check if first line looks like a header
        var hasHeader = lines[0].Contains(',') && 
                       (lines[0].ToLowerInvariant().Contains("data") || 
                        lines[0].ToLowerInvariant().Contains("symbology") ||
                        lines[0].ToLowerInvariant().Contains("quantity"));

        var startIndex = hasHeader ? 1 : 0;
        var items = new List<BarcodeItem>();

        // Try to detect column positions from header if present
        int dataColumn = 0;
        int symbologyColumn = -1;
        int quantityColumn = -1;
        List<string>? headerColumns = null;

        if (hasHeader && lines.Length > 0)
        {
            headerColumns = ParseCsvLine(lines[0]);
            for (int i = 0; i < headerColumns.Count; i++)
            {
                var col = headerColumns[i].ToLowerInvariant().Trim();
                if (col.Contains("data") || col.Contains("value") || col.Contains("code"))
                {
                    dataColumn = i;
                }
                else if (col.Contains("symbology") || col.Contains("type") || col.Contains("format"))
                {
                    symbologyColumn = i;
                }
                else if (col.Contains("quantity") || col.Contains("qty") || col.Contains("count"))
                {
                    quantityColumn = i;
                }
            }
        }

        // Parse data rows
        for (int i = startIndex; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var columns = ParseCsvLine(lines[i]);
            if (columns.Count == 0)
                continue;

            // Get data (first column if no header, or from detected column)
            var data = dataColumn < columns.Count ? columns[dataColumn].Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(data))
                continue;

            // Get symbology
            var symbology = defaultSymbology;
            if (symbologyColumn >= 0 && symbologyColumn < columns.Count)
            {
                var symbologyStr = columns[symbologyColumn].Trim();
                if (Enum.TryParse<BarcodeSymbology>(symbologyStr, true, out var parsedSymbology))
                {
                    symbology = parsedSymbology;
                }
            }

            // Get quantity
            var quantity = 1;
            if (quantityColumn >= 0 && quantityColumn < columns.Count)
            {
                if (int.TryParse(columns[quantityColumn].Trim(), out var parsedQuantity) && parsedQuantity > 0)
                {
                    quantity = parsedQuantity;
                }
            }

            // Try to parse optional per-item settings
            int? moduleWidth = null;
            int? barcodeHeight = null;
            bool? showText = null;

            // Look for additional columns (modulewidth, height, showtext)
            for (int col = 0; col < columns.Count; col++)
            {
                if (col == dataColumn || col == symbologyColumn || col == quantityColumn)
                    continue;

                var colLower = headerColumns != null && col < headerColumns.Count 
                    ? headerColumns[col].ToLowerInvariant().Trim() 
                    : string.Empty;

                if (colLower.Contains("module") || colLower.Contains("width"))
                {
                    if (int.TryParse(columns[col].Trim(), out var parsedWidth) && parsedWidth > 0)
                    {
                        moduleWidth = parsedWidth;
                    }
                }
                else if (colLower.Contains("height"))
                {
                    if (int.TryParse(columns[col].Trim(), out var parsedHeight) && parsedHeight > 0)
                    {
                        barcodeHeight = parsedHeight;
                    }
                }
                else if (colLower.Contains("showtext") || colLower.Contains("text"))
                {
                    if (bool.TryParse(columns[col].Trim(), out var parsedShowText))
                    {
                        showText = parsedShowText;
                    }
                }
            }

            items.Add(new BarcodeItem
            {
                Data = data,
                Symbology = symbology,
                Quantity = quantity,
                ModuleWidth = moduleWidth,
                BarcodeHeight = barcodeHeight,
                ShowText = showText
            });
        }

        return items;
    }

    public static bool IsCsvFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension == ".csv")
        {
            return true;
        }

        // Check first few lines for comma separators
        try
        {
            var lines = File.ReadLines(filePath).Take(5);
            return lines.Any(line => line.Contains(',') && line.Split(',').Length > 1);
        }
        catch
        {
            return false;
        }
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = string.Empty;
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    current += '"';
                    i++; // Skip next quote
                }
                else
                {
                    // Toggle quote state
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // End of field
                result.Add(current);
                current = string.Empty;
            }
            else
            {
                current += c;
            }
        }

        // Add last field
        result.Add(current);

        return result;
    }
}

