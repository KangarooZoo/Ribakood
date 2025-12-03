using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using BarcodeApp.Models;

namespace BarcodeApp.Views;

public partial class BatchPrintOptionsDialog : Window, INotifyPropertyChanged
{
    private BatchPrintOptions _options;
    private int _totalItems;
    private int _startIndex;
    private int _endIndex;
    private bool _continueOnError;
    private bool _skipPrintedItems;

    public BatchPrintOptionsDialog(BatchPrintOptions currentOptions, int totalItems)
    {
        InitializeComponent();
        
        _totalItems = totalItems;
        _startIndex = currentOptions.StartIndex;
        _endIndex = currentOptions.EndIndex == int.MaxValue ? totalItems - 1 : currentOptions.EndIndex;
        _continueOnError = currentOptions.ContinueOnError;
        _skipPrintedItems = currentOptions.SkipPrintedItems;
        
        _options = new BatchPrintOptions
        {
            StartIndex = _startIndex,
            EndIndex = _endIndex,
            ContinueOnError = _continueOnError,
            SkipPrintedItems = _skipPrintedItems
        };
        
        DataContext = this;
        
        // Apply light theme to this window's resources
        ApplyLightTheme();
    }
    
    public BatchPrintOptions Options => _options;
    
    public int TotalItems
    {
        get => _totalItems;
        set => SetProperty(ref _totalItems, value);
    }
    
    public int StartIndex
    {
        get => _startIndex;
        set
        {
            if (SetProperty(ref _startIndex, value))
            {
                _options.StartIndex = value;
            }
        }
    }
    
    public int EndIndex
    {
        get => _endIndex;
        set
        {
            if (SetProperty(ref _endIndex, value))
            {
                _options.EndIndex = value;
            }
        }
    }
    
    public bool ContinueOnError
    {
        get => _continueOnError;
        set
        {
            if (SetProperty(ref _continueOnError, value))
            {
                _options.ContinueOnError = value;
            }
        }
    }
    
    public bool SkipPrintedItems
    {
        get => _skipPrintedItems;
        set
        {
            if (SetProperty(ref _skipPrintedItems, value))
            {
                _options.SkipPrintedItems = value;
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    private void ApplyLightTheme()
    {
        if (Resources != null)
        {
            Resources["MaterialDesignPaper"] = new SolidColorBrush(Colors.White);
            Resources["MaterialDesignBody"] = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21));
            Resources["MaterialDesignDivider"] = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
            Resources["MaterialDesignBackground"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
            Resources["MaterialDesignSecondaryTextBrush"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
        }
    }
    
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

