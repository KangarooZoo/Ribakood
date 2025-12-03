using System;
using System.Threading.Tasks;
using BarcodeApp.Models;

namespace BarcodeApp.Services;

public interface ILicensingService
{
    event EventHandler<LicenseStatusEventArgs>? LicenseStatusChanged;
    LicenseStatus GetLicenseStatus();
    Task<bool> ValidateLicenseAsync(string licenseKey);
    string GetMachineId();
}

public class LicenseStatusEventArgs : EventArgs
{
    public LicenseStatus Status { get; set; } = new();
}

public class LicenseStatus
{
    public bool IsValid { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsGracePeriod { get; set; }
    public string Message { get; set; } = string.Empty;
}

