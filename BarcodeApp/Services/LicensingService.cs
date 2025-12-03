using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BarcodeApp.Models;

namespace BarcodeApp.Services;

public class LicensingService : ILicensingService
{
    private readonly MachineIdProvider _machineIdProvider;
    private readonly string _licenseFilePath;
    private readonly HttpClient _httpClient;
    // License server endpoint
    // Update this if you deploy your own server
    private const string LicenseServerUrl = "https://v0-license-server-setup.vercel.app/api/validateKey";
    private const int GracePeriodDays = 7;

    public event EventHandler<LicenseStatusEventArgs>? LicenseStatusChanged;

    public LicensingService()
    {
        _machineIdProvider = new MachineIdProvider();
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BarcodeApp");
        Directory.CreateDirectory(appDataPath);
        _licenseFilePath = Path.Combine(appDataPath, "license.dat");
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null // Use PascalCase (default for .NET)
    };

    public string GetMachineId()
    {
        return _machineIdProvider.GetMachineId();
    }

    public LicenseStatus GetLicenseStatus()
    {
        try
        {
            var token = LoadLicenseToken();
            if (token == null)
            {
                return new LicenseStatus { IsValid = false, Message = "No license found" };
            }

            // Check if machine ID matches
            if (token.MachineId != GetMachineId())
            {
                return new LicenseStatus { IsValid = false, Message = "License not valid for this machine" };
            }

            // Check expiry
            if (token.ExpiryDate < DateTime.UtcNow)
            {
                return new LicenseStatus { IsValid = false, Message = "License expired" };
            }

            // Check grace period
            if (token.LastSuccessfulValidation.HasValue)
            {
                var daysSinceValidation = (DateTime.UtcNow - token.LastSuccessfulValidation.Value).TotalDays;
                if (daysSinceValidation > GracePeriodDays)
                {
                    return new LicenseStatus
                    {
                        IsValid = false,
                        IsGracePeriod = false,
                        Message = "Grace period expired"
                    };
                }
                if (daysSinceValidation > 0)
                {
                    return new LicenseStatus
                    {
                        IsValid = true,
                        IsGracePeriod = true,
                        ExpiryDate = token.ExpiryDate,
                        Message = "Offline grace period active"
                    };
                }
            }

            return new LicenseStatus
            {
                IsValid = true,
                ExpiryDate = token.ExpiryDate,
                Message = "License valid"
            };
        }
        catch
        {
            return new LicenseStatus { IsValid = false, Message = "Error checking license" };
        }
    }

    public async Task<bool> ValidateLicenseAsync(string licenseKey)
    {
        var machineId = GetMachineId();
        var maxRetries = 4;
        var delay = 1000; // Start with 1 second

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var request = new
                {
                    Key = licenseKey,
                    MachineId = machineId,
                    AppVersion = "1.0"
                };

                // Use explicit JSON serialization to ensure PascalCase
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(LicenseServerUrl, content);
                
                // Parse response even if status code indicates error (server may return error status with JSON body)
                LicenseValidationResponse? result = null;
                try
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<LicenseValidationResponse>(responseContent, JsonOptions);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse response: {ex.Message}");
                    // Try to read raw content for debugging
                    var rawContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Raw response: {rawContent}");
                }
                
                // Only proceed if we got a valid response with IsValid = true
                if (result != null && result.IsValid)
                {
                    // Validate that expiry date is reasonable (not default/min value)
                    if (result.ExpiryDate <= DateTime.MinValue.AddDays(1) || result.ExpiryDate <= DateTime.UtcNow)
                    {
                        // Invalid expiry date in response
                        System.Diagnostics.Debug.WriteLine($"Invalid expiry date in response: {result.ExpiryDate}");
                        return false;
                    }

                    var token = new LicenseToken
                    {
                        Key = licenseKey,
                        MachineId = machineId,
                        ExpiryDate = result.ExpiryDate,
                        IssuedAt = DateTime.UtcNow,
                        LastSuccessfulValidation = DateTime.UtcNow
                    };

                    SaveLicenseToken(token);
                    OnLicenseStatusChanged();
                    return true;
                }

                // If response indicates error, log it for debugging
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine($"License validation failed: {result.StatusMessage ?? "Unknown error"} (HTTP {response.StatusCode}, IsValid: {result.IsValid})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"License validation failed: Could not parse response (HTTP {response.StatusCode})");
                }

                return false;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error on attempt {attempt + 1}: {ex.Message}");
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(delay);
                    delay *= 2; // Exponential backoff
                }
                else
                {
                    // On final failure, check if we have a valid local token for grace period
                    var localToken = LoadLicenseToken();
                    if (localToken != null && localToken.MachineId == machineId && localToken.ExpiryDate > DateTime.UtcNow)
                    {
                        // Update last validation attempt but don't mark as successful
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during license validation: {ex.Message}");
                return false;
            }
        }

        return false;
    }

    private void SaveLicenseToken(LicenseToken token)
    {
        try
        {
            var json = JsonSerializer.Serialize(token);
            var encrypted = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(json),
                null,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);

            File.WriteAllBytes(_licenseFilePath, encrypted);
        }
        catch
        {
            // Handle encryption/save errors
        }
    }

    private LicenseToken? LoadLicenseToken()
    {
        try
        {
            if (!File.Exists(_licenseFilePath))
            {
                return null;
            }

            var encrypted = File.ReadAllBytes(_licenseFilePath);
            var decrypted = ProtectedData.Unprotect(
                encrypted,
                null,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);

            var json = Encoding.UTF8.GetString(decrypted);
            return JsonSerializer.Deserialize<LicenseToken>(json);
        }
        catch
        {
            return null;
        }
    }

    private void OnLicenseStatusChanged()
    {
        var status = GetLicenseStatus();
        LicenseStatusChanged?.Invoke(this, new LicenseStatusEventArgs { Status = status });
    }
}

internal class LicenseValidationResponse
{
    public bool IsValid { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? StatusMessage { get; set; }
}

