using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using BarcodeApp.Models;
using BarcodeApp.Services;
using BarcodeApp.ViewModels;

namespace BarcodeApp.ViewModels;

public class BatchManagementViewModel : BaseViewModel
{
    private readonly IBarcodeService _barcodeService;
    private string _searchText = string.Empty;
    private BatchItemValidationResult? _selectedItem;
    private ObservableCollection<BatchItemValidationResult> _filteredItems = new();
    private string _currentFilter = string.Empty; // "valid", "invalid", or ""
    private List<BatchItemValidationResult> _validationResults = new();

    public BatchManagementViewModel(ObservableCollection<BarcodeItem> batchItems, List<BatchItemValidationResult> validationResults, IBarcodeService barcodeService)
    {
        _barcodeService = barcodeService;
        BatchItems = batchItems;
        ValidationResults = validationResults;
        
        UpdateFilteredItems();
        
        EditItemCommand = new RelayCommand(_ => EditSelectedItem(), _ => SelectedItem != null);
        RemoveItemCommand = new RelayCommand(_ => RemoveSelectedItem(), _ => SelectedItem != null);
        ClearAllCommand = new RelayCommand(_ => ClearAllItems(), _ => BatchItems.Any());
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
        PreviewBatchCommand = new RelayCommand(_ => PreviewRequested?.Invoke(this, EventArgs.Empty), _ => BatchItems.Any());
        ExportToCsvCommand = new RelayCommand(_ => ExportToCsv(), _ => BatchItems.Any());
        ExportToTextCommand = new RelayCommand(_ => ExportToText(), _ => BatchItems.Any());
        ExportValidationResultsCommand = new RelayCommand(_ => ExportValidationResults(), _ => _validationResults.Any());
        SortByDataCommand = new RelayCommand(_ => SortByData(), _ => BatchItems.Any());
        SortBySymbologyCommand = new RelayCommand(_ => SortBySymbology(), _ => BatchItems.Any());
        RemoveDuplicatesCommand = new RelayCommand(_ => RemoveDuplicates(), _ => BatchItems.Any());
        FilterValidOnlyCommand = new RelayCommand(_ => FilterValidOnly(), _ => _validationResults.Any());
        FilterInvalidOnlyCommand = new RelayCommand(_ => FilterInvalidOnly(), _ => _validationResults.Any(r => !r.IsValid));
        ClearFilterCommand = new RelayCommand(_ => ClearFilter(), _ => !string.IsNullOrWhiteSpace(SearchText));
    }

    public ObservableCollection<BarcodeItem> BatchItems { get; }
    public List<BatchItemValidationResult> ValidationResults 
    { 
        get => _validationResults;
        private set => _validationResults = value;
    }

    public ObservableCollection<BatchItemValidationResult> FilteredItems
    {
        get => _filteredItems;
        private set => SetProperty(ref _filteredItems, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                UpdateFilteredItems();
                ((RelayCommand)ClearFilterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public BatchItemValidationResult? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public int TotalItems => BatchItems.Count;
    public int ValidItems => _validationResults.Count(r => r.IsValid);
    public int InvalidItems => _validationResults.Count(r => !r.IsValid);

    public ICommand EditItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand PreviewBatchCommand { get; }
    public ICommand ExportToCsvCommand { get; }
    public ICommand ExportToTextCommand { get; }
    public ICommand ExportValidationResultsCommand { get; }
    public ICommand SortByDataCommand { get; }
    public ICommand SortBySymbologyCommand { get; }
    public ICommand RemoveDuplicatesCommand { get; }
    public ICommand FilterValidOnlyCommand { get; }
    public ICommand FilterInvalidOnlyCommand { get; }
    public ICommand ClearFilterCommand { get; }

    public event EventHandler? CloseRequested;
    public event EventHandler<BarcodeItem>? ItemEdited;
    public event EventHandler? ItemsChanged;
    public event EventHandler? PreviewRequested;

    private void UpdateFilteredItems()
    {
        var filtered = _validationResults.AsEnumerable();
        
        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(r => r.Item.Data.ToLowerInvariant().Contains(searchLower));
        }
        
        // Apply status filter
        if (_currentFilter == "valid")
        {
            filtered = filtered.Where(r => r.IsValid);
        }
        else if (_currentFilter == "invalid")
        {
            filtered = filtered.Where(r => !r.IsValid);
        }

        FilteredItems = new ObservableCollection<BatchItemValidationResult>(filtered);
        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(ValidItems));
        OnPropertyChanged(nameof(InvalidItems));
    }
    
    private void SortByData()
    {
        var sorted = _validationResults.OrderBy(r => r.Item.Data).ToList();
        _validationResults = sorted;
        UpdateFilteredItems();
        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private void SortBySymbology()
    {
        var sorted = _validationResults.OrderBy(r => r.Item.Symbology).ThenBy(r => r.Item.Data).ToList();
        _validationResults = sorted;
        UpdateFilteredItems();
        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private void RemoveDuplicates()
    {
        var seen = new HashSet<string>();
        var itemsToRemove = new List<BarcodeItem>();
        
        foreach (var item in BatchItems.ToList())
        {
            var key = $"{item.Data}|{item.Symbology}";
            if (seen.Contains(key))
            {
                itemsToRemove.Add(item);
            }
            else
            {
                seen.Add(key);
            }
        }
        
        foreach (var item in itemsToRemove)
        {
            BatchItems.Remove(item);
            var result = _validationResults.FirstOrDefault(r => r.Item == item);
            if (result != null)
            {
                _validationResults.Remove(result);
            }
        }
        
        UpdateFilteredItems();
        ItemsChanged?.Invoke(this, EventArgs.Empty);
        
        if (itemsToRemove.Count > 0)
        {
            MessageBox.Show($"Removed {itemsToRemove.Count} duplicate item(s).", "Duplicates Removed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    private void FilterValidOnly()
    {
        _currentFilter = "valid";
        UpdateFilteredItems();
    }
    
    private void FilterInvalidOnly()
    {
        _currentFilter = "invalid";
        UpdateFilteredItems();
    }
    
    private void ClearFilter()
    {
        SearchText = string.Empty;
        _currentFilter = string.Empty;
        UpdateFilteredItems();
    }

    private void EditSelectedItem()
    {
        if (SelectedItem == null) return;
        
        // For now, just trigger the event - the dialog can handle editing
        ItemEdited?.Invoke(this, SelectedItem.Item);
    }

    private void RemoveSelectedItem()
    {
        if (SelectedItem == null) return;
        
        var item = SelectedItem.Item;
        BatchItems.Remove(item);
        _validationResults.Remove(SelectedItem);
        UpdateFilteredItems();
        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearAllItems()
    {
        BatchItems.Clear();
        _validationResults.Clear();
        UpdateFilteredItems();
        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private void ExportToCsv()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Export Batch to CSV",
            DefaultExt = "csv"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                BatchExportService.ExportToCsv(BatchItems, dialog.FileName);
                MessageBox.Show($"Successfully exported {BatchItems.Count} item(s) to CSV.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private void ExportToText()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Export Batch to Text",
            DefaultExt = "txt"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                BatchExportService.ExportToText(BatchItems, dialog.FileName);
                MessageBox.Show($"Successfully exported {BatchItems.Count} item(s) to text file.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to text: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private void ExportValidationResults()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Export Validation Results",
            DefaultExt = "csv"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                BatchExportService.ExportValidationResults(_validationResults, dialog.FileName);
                MessageBox.Show($"Successfully exported validation results for {_validationResults.Count} item(s).", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting validation results: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

