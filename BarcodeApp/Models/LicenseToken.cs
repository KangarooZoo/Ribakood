using System;

namespace BarcodeApp.Models;

public class LicenseToken
{
    public string Key { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? LastSuccessfulValidation { get; set; }
}

