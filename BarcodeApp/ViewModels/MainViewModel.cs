using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BarcodeApp.Models;
using BarcodeApp.Services;
using BarcodeApp.Views;
using Microsoft.Win32;

namespace BarcodeApp.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;
    private readonly IBarcodeService _barcodeService;
    private readonly IPrintingService _printingService;
    private readonly ILicensingService _licensingService;

    private bool _isDarkTheme;
    private ImageSource? _previewImage;
    private string _inputData = string.Empty;
    private BarcodeSymbology _selectedSymbology = BarcodeSymbology.Code128;
    private bool _isInputValid;
    private string _validationMessage = string.Empty;
    private bool _hasInputBeenEntered;
    private int _moduleWidth = 2;
    private int _barcodeHeight = 100;
    private bool _showText = true;
    private string _selectedPrinter = string.Empty;
    private LabelLayout _selectedLayout;
    private int _quantity = 1;
    private bool _isBatchMode;
    private string _batchFileName = string.Empty;
    private BatchPrintOptions _batchPrintOptions = new();
    private string _licenseStatusText = "Checking license...";
    private bool _isLicenseValid;
    private DateTime? _licenseExpiry;
    private LicenseViewModel? _licenseViewModel;

    public MainViewModel(
        IThemeService themeService,
        IBarcodeService barcodeService,
        IPrintingService printingService,
        ILicensingService licensingService)
    {
        _themeService = themeService;
        _barcodeService = barcodeService;
        _printingService = printingService;
        _licensingService = licensingService;

        _isDarkTheme = _themeService.IsDarkTheme;
        _selectedLayout = AvailableLayouts.FirstOrDefault() ?? new LabelLayout { Name = "A4", Width = 210, Height = 297 };

        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        LoadBatchFileCommand = new RelayCommand(_ => LoadBatchFile());
        PrintNowCommand = new RelayCommand(_ => PrintNow(), _ => CanPrint());
        ShowLicenseModalCommand = new RelayCommand(_ => ShowLicenseModal());
        ValidateBatchCommand = new RelayCommand(_ => ValidateBatchItems());
        ManageBatchCommand = new RelayCommand(_ => ShowBatchManagement(), _ => IsBatchMode && BatchItems.Any());
        ClearBatchCommand = new RelayCommand(_ => ClearBatch(), _ => IsBatchMode && BatchItems.Any());
        ShowPrintOptionsCommand = new RelayCommand(_ => ShowPrintOptions(), _ => IsBatchMode && BatchItems.Any());
        
        // Initialize license view model
        _licenseViewModel = new LicenseViewModel(_licensingService);
        _licenseViewModel.ActivationCompleted += (sender, success) =>
        {
            if (success)
            {
                UpdateLicenseStatus();
            }
        };

        _licensingService.LicenseStatusChanged += OnLicenseStatusChanged;
        UpdateLicenseStatus();

        AvailablePrinters = new ObservableCollection<string>(_printingService.GetAvailablePrinters());
        if (AvailablePrinters.Any())
        {
            SelectedPrinter = AvailablePrinters.First();
        }

        UpdatePreview();
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                ToggleTheme();
            }
        }
    }

    public ImageSource? PreviewImage
    {
        get => _previewImage;
        set => SetProperty(ref _previewImage, value);
    }

    public string InputData
    {
        get => _inputData;
        set
        {
            if (SetProperty(ref _inputData, value))
            {
                // Mark that user has started entering input
                if (!string.IsNullOrWhiteSpace(value) && !_hasInputBeenEntered)
                {
                    _hasInputBeenEntered = true;
                    OnPropertyChanged(nameof(HasInputBeenEntered));
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    _hasInputBeenEntered = false;
                    OnPropertyChanged(nameof(HasInputBeenEntered));
                }
                
                ValidateInput();
                UpdatePreview();
            }
        }
    }

    public bool HasInputBeenEntered
    {
        get => _hasInputBeenEntered;
        private set => SetProperty(ref _hasInputBeenEntered, value);
    }

    public BarcodeSymbology SelectedSymbology
    {
        get => _selectedSymbology;
        set
        {
            if (SetProperty(ref _selectedSymbology, value))
            {
                ValidateInput();
                UpdatePreview();
            }
        }
    }

    public bool IsInputValid
    {
        get => _isInputValid;
        set => SetProperty(ref _isInputValid, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    public int ModuleWidth
    {
        get => _moduleWidth;
        set
        {
            if (SetProperty(ref _moduleWidth, value))
            {
                UpdatePreview(); // Update immediately when settings change
            }
        }
    }

    public int BarcodeHeight
    {
        get => _barcodeHeight;
        set
        {
            if (SetProperty(ref _barcodeHeight, value))
            {
                UpdatePreview(); // Update immediately when settings change
            }
        }
    }

    public bool ShowText
    {
        get => _showText;
        set
        {
            if (SetProperty(ref _showText, value))
            {
                UpdatePreview(); // Update immediately when settings change
            }
        }
    }

    public string SelectedPrinter
    {
        get => _selectedPrinter;
        set => SetProperty(ref _selectedPrinter, value);
    }

    public ObservableCollection<string> AvailablePrinters { get; }

    public LabelLayout SelectedLayout
    {
        get => _selectedLayout;
        set => SetProperty(ref _selectedLayout, value);
    }

    public ObservableCollection<LabelLayout> AvailableLayouts { get; } = new()
    {
        new LabelLayout { Name = "A4", Width = 210, Height = 297 },
        new LabelLayout { Name = "2x1 inch", Width = 50.8, Height = 25.4 },
        new LabelLayout { Name = "4x2 inch", Width = 101.6, Height = 50.8 }
    };

    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    public bool IsBatchMode
    {
        get => _isBatchMode;
        set => SetProperty(ref _isBatchMode, value);
    }

    public ObservableCollection<BarcodeItem> BatchItems { get; } = new();
    
    private List<BatchItemValidationResult> _batchValidationResults = new();
    
    public List<BatchItemValidationResult> BatchValidationResults
    {
        get => _batchValidationResults;
        private set => SetProperty(ref _batchValidationResults, value);
    }
    
    public int ValidBatchItemsCount => BatchValidationResults.Count(r => r.IsValid);
    public int InvalidBatchItemsCount => BatchValidationResults.Count(r => !r.IsValid);
    
    public string BatchFileName
    {
        get => _batchFileName;
        private set => SetProperty(ref _batchFileName, value);
    }

    public string LicenseStatusText
    {
        get => _licenseStatusText;
        set => SetProperty(ref _licenseStatusText, value);
    }

    public bool IsLicenseValid
    {
        get => _isLicenseValid;
        set
        {
            if (SetProperty(ref _isLicenseValid, value))
            {
                ((RelayCommand)PrintNowCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public DateTime? LicenseExpiry
    {
        get => _licenseExpiry;
        set => SetProperty(ref _licenseExpiry, value);
    }

    public ICommand ToggleThemeCommand { get; }
    public ICommand LoadBatchFileCommand { get; }
    public ICommand PrintNowCommand { get; }
    public ICommand ShowLicenseModalCommand { get; }
    public ICommand ValidateBatchCommand { get; }
    public ICommand ManageBatchCommand { get; }
    public ICommand ClearBatchCommand { get; }
    public ICommand ShowPrintOptionsCommand { get; }
    
    public BatchPrintOptions BatchPrintOptions
    {
        get => _batchPrintOptions;
        set => SetProperty(ref _batchPrintOptions, value);
    }
    
    public LicenseViewModel? LicenseViewModel
    {
        get => _licenseViewModel;
        private set => SetProperty(ref _licenseViewModel, value);
    }

    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        _isDarkTheme = _themeService.IsDarkTheme;
        OnPropertyChanged(nameof(IsDarkTheme));
    }

    private void ValidateInput()
    {
        var (isValid, message) = BarcodeValidation.Validate(InputData, SelectedSymbology);
        IsInputValid = isValid;
        ValidationMessage = message;
    }

    private void UpdatePreview()
    {
        // Ensure we're on the UI thread
        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() == false)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(UpdatePreview);
            return;
        }

        // Clear preview if input is empty
        if (string.IsNullOrWhiteSpace(InputData))
        {
            PreviewImage = null;
            return;
        }

        // Always try to generate preview, even if validation fails
        // This allows users to see what the barcode would look like
        try
        {
            System.Diagnostics.Debug.WriteLine($"UpdatePreview called: InputData='{InputData}', Symbology={SelectedSymbology}, ModuleWidth={ModuleWidth}, Height={BarcodeHeight}");
            var image = _barcodeService.GenerateBarcode(InputData, SelectedSymbology, ModuleWidth, BarcodeHeight, ShowText);
            System.Diagnostics.Debug.WriteLine($"Barcode generation result: {(image != null ? "SUCCESS - Image created" : "FAILED - Image is null")}");
            
            if (image != null)
            {
                PreviewImage = image;
                System.Diagnostics.Debug.WriteLine($"PreviewImage set successfully. Width={image.Width}, Height={image.Height}");
            }
            else
            {
                PreviewImage = null;
                System.Diagnostics.Debug.WriteLine("PreviewImage set to null because generation returned null");
            }
        }
        catch (Exception ex)
        {
            // If generation fails, set to null so placeholder shows
            PreviewImage = null;
            System.Diagnostics.Debug.WriteLine($"Preview generation error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void LoadBatchFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Select batch file",
            InitialDirectory = !string.IsNullOrEmpty(Properties.Settings.Default.LastBatchFileLocation) 
                ? Properties.Settings.Default.LastBatchFileLocation 
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                BatchItems.Clear();
                BatchFileName = Path.GetFileName(dialog.FileName);
                
                // Check if file is CSV format
                if (CsvParser.IsCsvFile(dialog.FileName))
                {
                    // Parse as CSV
                    var items = CsvParser.ParseCsvFile(dialog.FileName, SelectedSymbology);
                    foreach (var item in items)
                    {
                        BatchItems.Add(item);
                    }
                }
                else
                {
                    // Parse as plain text (one item per line)
                    var lines = File.ReadAllLines(dialog.FileName);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            BatchItems.Add(new BarcodeItem
                            {
                                Data = line.Trim(),
                                Symbology = SelectedSymbology,
                                Quantity = 1
                            });
                        }
                    }
                }
                
                IsBatchMode = BatchItems.Count > 0;
                
                // Save last batch file location
                Properties.Settings.Default.LastBatchFileLocation = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                Properties.Settings.Default.Save();
                
                // Validate batch items after loading (if auto-validate is enabled)
                if (IsBatchMode)
                {
                    if (Properties.Settings.Default.AutoValidateBatchOnLoad)
                    {
                        ValidateBatchItems();
                    }
                    ShowSuccessMessage("Batch File Loaded", $"Successfully loaded {BatchItems.Count} item(s) from batch file.\nValid: {ValidBatchItemsCount}, Invalid: {InvalidBatchItemsCount}");
                }
                else
                {
                    ShowErrorMessage("Empty Batch File", "The selected file contains no valid items.", "Please ensure the file contains at least one non-empty line.");
                }
            }
            catch (FileNotFoundException)
            {
                ShowErrorMessage("File Not Found", "The selected file could not be found.", "Please check that the file exists and try again.");
            }
            catch (UnauthorizedAccessException)
            {
                ShowErrorMessage("Access Denied", "You do not have permission to access this file.", "Please check file permissions and try again.");
            }
            catch (IOException ex)
            {
                ShowErrorMessage("File Read Error", "An error occurred while reading the file.", ex.Message);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error Loading Batch File", "An unexpected error occurred while loading the batch file.", ex.Message);
            }
        }
    }
    
    private void ShowErrorMessage(string title, string message, string? details = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var errorViewModel = new BatchErrorViewModel(title, message, details);
            var errorDialog = new BatchErrorDialog(errorViewModel)
            {
                Owner = Application.Current.MainWindow
            };
            errorDialog.ShowDialog();
        });
    }
    
    private void ShowSuccessMessage(string title, string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    private bool CanPrint()
    {
        return IsLicenseValid && (IsInputValid || (IsBatchMode && BatchItems.Any()));
    }

    private async void PrintNow()
    {
        try
        {
            if (IsBatchMode && BatchItems.Any())
            {
                // Apply print options
                var startIndex = BatchPrintOptions.StartIndex;
                var endIndex = BatchPrintOptions.EndIndex == int.MaxValue ? BatchItems.Count - 1 : BatchPrintOptions.EndIndex;
                var continueOnError = BatchPrintOptions.ContinueOnError;
                
                // Filter items if skip printed is enabled (for now, just use all items)
                var itemsToPrint = BatchItems.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
                
                if (itemsToPrint.Count == 0)
                {
                    ShowErrorMessage("No Items to Print", "The selected range contains no items.", null);
                    return;
                }
                
                var progressViewModel = new BatchProgressViewModel();
                var progressDialog = new BatchProgressDialog(progressViewModel)
                {
                    Owner = Application.Current.MainWindow
                };

                progressViewModel.IsPrinting = true;
                progressViewModel.UpdateProgress(0, itemsToPrint.Count, string.Empty, "Starting print...");

                // Subscribe to progress events
                EventHandler<PrintProgressEventArgs>? progressHandler = (sender, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressViewModel.UpdateProgress(e.CurrentItem, e.TotalItems, e.CurrentItemData, e.Status);
                    });
                };
                _printingService.PrintProgress += progressHandler;

                // Show progress dialog
                var dialogTask = Task.Run(() => Application.Current.Dispatcher.Invoke(() => progressDialog.ShowDialog()));

                try
                {
                    await _printingService.PrintBatchAsync(itemsToPrint, SelectedPrinter, SelectedLayout, ModuleWidth, BarcodeHeight, ShowText, startIndex, endIndex, continueOnError, progressViewModel.CancellationToken);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressDialog.Close();
                        ShowSuccessMessage("Batch Print Complete", $"Successfully printed {itemsToPrint.Count} item(s).");
                    });
                }
                catch (OperationCanceledException)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressDialog.Close();
                        ShowErrorMessage("Print Cancelled", "The print operation was cancelled.", null);
                    });
                }
                finally
                {
                    _printingService.PrintProgress -= progressHandler;
                    progressViewModel.Reset();
                }
            }
            else if (IsInputValid)
            {
                var item = new BarcodeItem
                {
                    Data = InputData,
                    Symbology = SelectedSymbology,
                    Quantity = Quantity
                };
                await _printingService.PrintAsync(item, SelectedPrinter, SelectedLayout, ModuleWidth, BarcodeHeight, ShowText);
                ShowSuccessMessage("Print Complete", "Barcode printed successfully.");
            }
        }
        catch (System.Drawing.Printing.InvalidPrinterException)
        {
            ShowErrorMessage("Printer Error", "The selected printer is not available.", "Please select a different printer and try again.");
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Print Error", "An error occurred while printing.", ex.Message);
        }
    }

    private void OnLicenseStatusChanged(object? sender, LicenseStatusEventArgs e)
    {
        UpdateLicenseStatus();
    }

    private void UpdateLicenseStatus()
    {
        var status = _licensingService.GetLicenseStatus();
        IsLicenseValid = status.IsValid;
        LicenseExpiry = status.ExpiryDate;

        if (status.IsValid && status.ExpiryDate.HasValue)
        {
            LicenseStatusText = $"Active until {status.ExpiryDate.Value:yyyy-MM-dd}";
        }
        else if (status.IsGracePeriod)
        {
            LicenseStatusText = "Offline Grace Period";
        }
        else
        {
            LicenseStatusText = "License Required";
        }
    }
    
    private void ShowLicenseModal()
    {
        if (_licenseViewModel == null)
        {
            _licenseViewModel = new LicenseViewModel(_licensingService);
            _licenseViewModel.ActivationCompleted += (sender, success) =>
            {
                if (success)
                {
                    UpdateLicenseStatus();
                }
            };
        }

        var licenseDialog = new LicenseDialog(_licenseViewModel)
        {
            Owner = Application.Current.MainWindow
        };
        licenseDialog.ShowDialog();
    }
    
    private void ValidateBatchItems()
    {
        if (!BatchItems.Any())
        {
            BatchValidationResults = new List<BatchItemValidationResult>();
            return;
        }

        BatchValidationResults = BarcodeValidation.ValidateBatch(BatchItems);
        OnPropertyChanged(nameof(ValidBatchItemsCount));
        OnPropertyChanged(nameof(InvalidBatchItemsCount));
    }
    
    private void ShowBatchManagement()
    {
        if (!BatchItems.Any()) return;
        
        // Ensure validation is up to date
        ValidateBatchItems();
        
        var batchViewModel = new BatchManagementViewModel(BatchItems, BatchValidationResults, _barcodeService);
        
        batchViewModel.ItemsChanged += (sender, e) =>
        {
            // Re-validate after changes
            ValidateBatchItems();
            batchViewModel = new BatchManagementViewModel(BatchItems, BatchValidationResults, _barcodeService);
        };
        
        batchViewModel.PreviewRequested += (sender, e) =>
        {
            // Show preview dialog
            var previewViewModel = new BatchPreviewViewModel(BatchItems, _barcodeService, ModuleWidth, BarcodeHeight, ShowText);
            var previewDialog = new BatchPreviewDialog(previewViewModel)
            {
                Owner = Application.Current.MainWindow
            };
            previewDialog.ShowDialog();
        };
        
        var batchDialog = new BatchManagementDialog(batchViewModel)
        {
            Owner = Application.Current.MainWindow
        };
        batchDialog.ShowDialog();
        
        // Update batch mode status after dialog closes
        IsBatchMode = BatchItems.Any();
        ((RelayCommand)ManageBatchCommand).RaiseCanExecuteChanged();
        ((RelayCommand)PrintNowCommand).RaiseCanExecuteChanged();
        ((RelayCommand)ClearBatchCommand).RaiseCanExecuteChanged();
    }
    
    private void ClearBatch()
    {
        BatchItems.Clear();
        BatchValidationResults.Clear();
        BatchFileName = string.Empty;
        BatchPrintOptions = new BatchPrintOptions();
        IsBatchMode = false;
        OnPropertyChanged(nameof(ValidBatchItemsCount));
        OnPropertyChanged(nameof(InvalidBatchItemsCount));
        ((RelayCommand)ManageBatchCommand).RaiseCanExecuteChanged();
        ((RelayCommand)PrintNowCommand).RaiseCanExecuteChanged();
        ((RelayCommand)ClearBatchCommand).RaiseCanExecuteChanged();
    }
    
    private void ShowPrintOptions()
    {
        if (!BatchItems.Any()) return;
        
        var optionsDialog = new BatchPrintOptionsDialog(BatchPrintOptions, BatchItems.Count)
        {
            Owner = Application.Current.MainWindow
        };
        
        if (optionsDialog.ShowDialog() == true)
        {
            BatchPrintOptions = optionsDialog.Options;
        }
    }
}


