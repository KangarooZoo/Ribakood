using System;
using System.Windows.Input;
using BarcodeApp.Services;

namespace BarcodeApp.ViewModels;

public class LicenseViewModel : BaseViewModel
{
    private readonly ILicensingService _licensingService;
    private string _licenseKey = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _licenseStatusText = string.Empty;

    public LicenseViewModel(ILicensingService licensingService)
    {
        _licensingService = licensingService;
        MachineIdDisplay = _licensingService.GetMachineId();
        ActivateCommand = new RelayCommand(async _ => await ActivateAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(LicenseKey));
        RetryCommand = new RelayCommand(async _ => await ActivateAsync(), _ => !IsBusy);
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    public string LicenseKey
    {
        get => _licenseKey;
        set
        {
            if (SetProperty(ref _licenseKey, value))
            {
                ((RelayCommand)ActivateCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ((RelayCommand)ActivateCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RetryCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string MachineIdDisplay { get; }

    public string LicenseStatusText
    {
        get => _licenseStatusText;
        set => SetProperty(ref _licenseStatusText, value);
    }

    public ICommand ActivateCommand { get; }
    public ICommand RetryCommand { get; }
    public ICommand CloseCommand { get; }

    public event EventHandler<bool>? ActivationCompleted;
    public event EventHandler? CloseRequested;

    private async System.Threading.Tasks.Task ActivateAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        LicenseStatusText = "Validating license...";

        try
        {
            var success = await _licensingService.ValidateLicenseAsync(LicenseKey);
            if (success)
            {
                LicenseStatusText = "License activated successfully!";
                ActivationCompleted?.Invoke(this, true);
            }
            else
            {
                ErrorMessage = "Invalid license key or activation failed. Please check your key and try again.";
                LicenseStatusText = "Activation failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during activation: {ex.Message}";
            LicenseStatusText = "Activation error";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

