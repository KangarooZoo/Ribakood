using System;

namespace BarcodeApp.Models;

public class BarcodeItem
{
    public string Data { get; set; } = string.Empty;
    public BarcodeSymbology Symbology { get; set; } = BarcodeSymbology.Code128;
    public int Quantity { get; set; } = 1;
    
    // Optional per-item settings (null = use global settings)
    public int? ModuleWidth { get; set; }
    public int? BarcodeHeight { get; set; }
    public bool? ShowText { get; set; }
}

