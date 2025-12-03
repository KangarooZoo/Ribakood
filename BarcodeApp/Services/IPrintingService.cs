using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BarcodeApp.Models;

namespace BarcodeApp.Services;

public interface IPrintingService
{
    event EventHandler<PrintProgressEventArgs>? PrintProgress;
    IEnumerable<string> GetAvailablePrinters();
    Task PrintAsync(BarcodeItem item, string printerName, LabelLayout layout, int moduleWidth, int height, bool showText);
    Task PrintBatchAsync(IEnumerable<BarcodeItem> items, string printerName, LabelLayout layout, int moduleWidth, int height, bool showText, CancellationToken cancellationToken = default);
    Task PrintBatchAsync(IEnumerable<BarcodeItem> items, string printerName, LabelLayout layout, int moduleWidth, int height, bool showText, int startIndex, int endIndex, bool continueOnError, CancellationToken cancellationToken = default);
}

public class PrintProgressEventArgs : EventArgs
{
    public int CurrentItem { get; set; }
    public int TotalItems { get; set; }
    public string CurrentItemData { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

