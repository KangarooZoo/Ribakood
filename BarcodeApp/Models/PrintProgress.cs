namespace BarcodeApp.Models;

public class PrintProgress
{
    public int CurrentItem { get; set; }
    public int TotalItems { get; set; }
    public string CurrentItemData { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    public double ProgressPercentage => TotalItems > 0 ? (double)CurrentItem / TotalItems * 100 : 0;
}

