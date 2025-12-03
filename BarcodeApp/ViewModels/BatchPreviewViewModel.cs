using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using BarcodeApp.Models;
using BarcodeApp.Services;
using BarcodeApp.ViewModels;

namespace BarcodeApp.ViewModels;

public class BatchPreviewViewModel : BaseViewModel
{
    private readonly IBarcodeService _barcodeService;
    private ImageSource? _selectedPreview;
    private BatchPreviewItem? _selectedItem;

    public BatchPreviewViewModel(IEnumerable<BarcodeItem> items, IBarcodeService barcodeService, int moduleWidth, int height, bool showText)
    {
        _barcodeService = barcodeService;
        
        PreviewItems = new ObservableCollection<BatchPreviewItem>();
        
        var itemsList = items.ToList();
        foreach (var item in itemsList)
        {
            // Skip items with empty or null data
            if (string.IsNullOrWhiteSpace(item.Data))
            {
                PreviewItems.Add(new BatchPreviewItem
                {
                    Item = item,
                    PreviewImage = null,
                    Index = PreviewItems.Count,
                    HasError = true,
                    ErrorMessage = "Data cannot be empty"
                });
                continue;
            }
            
            var itemModuleWidth = item.ModuleWidth ?? moduleWidth;
            var itemHeight = item.BarcodeHeight ?? height;
            var itemShowText = item.ShowText ?? showText;
            
            ImageSource? previewImage = null;
            string? errorMessage = null;
            
            try
            {
                previewImage = _barcodeService.GenerateBarcode(
                    item.Data, 
                    item.Symbology, 
                    itemModuleWidth, 
                    itemHeight, 
                    itemShowText);
                
                if (previewImage == null)
                {
                    errorMessage = "Failed to generate preview";
                    System.Diagnostics.Debug.WriteLine($"Preview generation returned null for item: {item.Data}, Symbology: {item.Symbology}");
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Preview generation error for item {item.Data}: {ex.Message}");
            }
            
            PreviewItems.Add(new BatchPreviewItem
            {
                Item = item,
                PreviewImage = previewImage,
                Index = PreviewItems.Count,
                HasError = previewImage == null,
                ErrorMessage = errorMessage ?? string.Empty
            });
        }
        
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    public ObservableCollection<BatchPreviewItem> PreviewItems { get; }

    public BatchPreviewItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                SelectedPreview = value?.PreviewImage;
            }
        }
    }

    public ImageSource? SelectedPreview
    {
        get => _selectedPreview;
        set => SetProperty(ref _selectedPreview, value);
    }

    public ICommand CloseCommand { get; }

    public event EventHandler? CloseRequested;
}

public class BatchPreviewItem
{
    public BarcodeItem Item { get; set; } = null!;
    public ImageSource? PreviewImage { get; set; }
    public int Index { get; set; }
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

