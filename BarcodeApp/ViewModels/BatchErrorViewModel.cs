using System.Windows.Input;
using BarcodeApp.ViewModels;

namespace BarcodeApp.ViewModels;

public class BatchErrorViewModel : BaseViewModel
{
    private string _errorTitle = string.Empty;
    private string _errorMessage = string.Empty;
    private string _errorDetails = string.Empty;

    public BatchErrorViewModel(string title, string message, string? details = null)
    {
        ErrorTitle = title;
        ErrorMessage = message;
        ErrorDetails = details ?? string.Empty;
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    public string ErrorTitle
    {
        get => _errorTitle;
        set => SetProperty(ref _errorTitle, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string ErrorDetails
    {
        get => _errorDetails;
        set => SetProperty(ref _errorDetails, value);
    }

    public ICommand CloseCommand { get; }

    public event EventHandler? CloseRequested;
}

