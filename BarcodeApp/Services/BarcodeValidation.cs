using System.Text.RegularExpressions;
using BarcodeApp.Models;

namespace BarcodeApp.Services;

public static class BarcodeValidation
{
    public static (bool IsValid, string Message) Validate(string data, BarcodeSymbology symbology)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return (false, "Data cannot be empty");
        }

        return symbology switch
        {
            BarcodeSymbology.Code128 => ValidateCode128(data),
            BarcodeSymbology.EAN13 => ValidateEAN13(data),
            BarcodeSymbology.Code39 => ValidateCode39(data),
            BarcodeSymbology.QRCode => ValidateQRCode(data),
            BarcodeSymbology.DataMatrix => ValidateDataMatrix(data),
            _ => (false, "Unknown symbology")
        };
    }

    private static (bool IsValid, string Message) ValidateCode128(string data)
    {
        // Code 128 supports ASCII 32-126 and special control characters
        if (data.Length > 0 && data.Length <= 80)
        {
            return (true, "Valid");
        }
        return (false, "Code 128 must be 1-80 characters");
    }

    private static (bool IsValid, string Message) ValidateEAN13(string data)
    {
        // EAN-13 requires exactly 12 or 13 digits (13 includes check digit)
        if (Regex.IsMatch(data, @"^\d{12}$"))
        {
            return (true, "Valid (12 digits, check digit will be calculated)");
        }
        if (Regex.IsMatch(data, @"^\d{13}$"))
        {
            return (true, "Valid");
        }
        return (false, "EAN-13 requires exactly 12 or 13 digits");
    }

    private static (bool IsValid, string Message) ValidateCode39(string data)
    {
        // Code 39 supports: 0-9, A-Z, space, and special characters: -.$/+%
        if (Regex.IsMatch(data, @"^[0-9A-Z\s\-\.\$\/\+\%]+$") && data.Length > 0)
        {
            return (true, "Valid");
        }
        return (false, "Code 39 supports only: 0-9, A-Z, space, and -.$/+%");
    }

    private static (bool IsValid, string Message) ValidateQRCode(string data)
    {
        // QR Code can handle most text, but has practical limits
        if (data.Length > 0 && data.Length <= 2953) // Alphanumeric mode limit
        {
            return (true, "Valid");
        }
        return (false, "QR Code data too long (max ~2953 alphanumeric characters)");
    }

    private static (bool IsValid, string Message) ValidateDataMatrix(string data)
    {
        // Data Matrix can handle most text
        if (data.Length > 0 && data.Length <= 2335) // Practical limit
        {
            return (true, "Valid");
        }
        return (false, "Data Matrix data too long (max ~2335 characters)");
    }

    public static List<BatchItemValidationResult> ValidateBatch(IEnumerable<BarcodeItem> items)
    {
        var results = new List<BatchItemValidationResult>();
        var index = 0;

        foreach (var item in items)
        {
            var (isValid, message) = Validate(item.Data, item.Symbology);
            results.Add(new BatchItemValidationResult
            {
                Item = item,
                IsValid = isValid,
                ErrorMessage = isValid ? string.Empty : message,
                Index = index
            });
            index++;
        }

        return results;
    }
}

