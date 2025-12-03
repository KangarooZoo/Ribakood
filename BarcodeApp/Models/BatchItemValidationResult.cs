using System;

namespace BarcodeApp.Models;

public class BatchItemValidationResult
{
    public BarcodeItem Item { get; set; } = null!;
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int Index { get; set; }
}

