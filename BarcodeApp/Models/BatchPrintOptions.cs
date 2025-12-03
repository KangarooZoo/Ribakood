namespace BarcodeApp.Models;

public class BatchPrintOptions
{
    public int StartIndex { get; set; } = 0;
    public int EndIndex { get; set; } = int.MaxValue;
    public bool ContinueOnError { get; set; } = false;
    public bool SkipPrintedItems { get; set; } = false;
}

