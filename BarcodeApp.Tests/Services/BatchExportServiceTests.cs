using System.IO;
using BarcodeApp.Models;
using BarcodeApp.Services;
using FluentAssertions;
using Xunit;

namespace BarcodeApp.Tests.Services;

public class BatchExportServiceTests
{
    private string CreateTempFile()
    {
        return Path.GetTempFileName();
    }

    [Fact]
    public void ExportToCsv_EmptyList_ExportsOnlyHeader()
    {
        // Arrange
        var items = new List<BarcodeItem>();
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToCsv(items, tempFile, includeHeader: true);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines.Should().HaveCount(1);
            lines[0].Should().Contain("Data,Symbology,Quantity");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithItems_ExportsCorrectly()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "ABC123", Symbology = BarcodeSymbology.Code128, Quantity = 1 },
            new() { Data = "DEF456", Symbology = BarcodeSymbology.QRCode, Quantity = 2 }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToCsv(items, tempFile, includeHeader: true);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines.Should().HaveCount(3);
            lines[0].Should().Contain("Data,Symbology,Quantity");
            lines[1].Should().Contain("ABC123");
            lines[1].Should().Contain("Code128");
            lines[2].Should().Contain("DEF456");
            lines[2].Should().Contain("QRCode");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithoutHeader_ExportsOnlyData()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "ABC123", Symbology = BarcodeSymbology.Code128, Quantity = 1 }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToCsv(items, tempFile, includeHeader: false);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines.Should().HaveCount(1);
            lines[0].Should().NotContain("Data,Symbology");
            lines[0].Should().Contain("ABC123");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithCommasInData_EscapesCorrectly()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "ABC,123", Symbology = BarcodeSymbology.Code128, Quantity = 1 }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToCsv(items, tempFile, includeHeader: false);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines[0].Should().Contain("\"ABC,123\"");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithQuotesInData_EscapesCorrectly()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "ABC\"123", Symbology = BarcodeSymbology.Code128, Quantity = 1 }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToCsv(items, tempFile, includeHeader: false);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines[0].Should().Contain("\"ABC\"\"123\"");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithOptionalFields_ExportsCorrectly()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() 
            { 
                Data = "ABC123", 
                Symbology = BarcodeSymbology.Code128, 
                Quantity = 2,
                ModuleWidth = 3,
                BarcodeHeight = 150,
                ShowText = true
            }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToCsv(items, tempFile, includeHeader: true);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines[1].Should().Contain("3");
            lines[1].Should().Contain("150");
            lines[1].Should().Contain("True");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToCsv_WithNullOptionalFields_ExportsEmpty()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() 
            { 
                Data = "ABC123", 
                Symbology = BarcodeSymbology.Code128, 
                Quantity = 1,
                ModuleWidth = null,
                BarcodeHeight = null,
                ShowText = null
            }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToCsv(items, tempFile, includeHeader: false);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            var fields = lines[0].Split(',');
            fields[3].Should().BeEmpty(); // ModuleWidth
            fields[4].Should().BeEmpty(); // BarcodeHeight
            fields[5].Should().BeEmpty(); // ShowText
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToText_EmptyList_ExportsEmptyFile()
    {
        // Arrange
        var items = new List<BarcodeItem>();
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToText(items, tempFile);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToText_WithItems_ExportsOnlyData()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "ABC123", Symbology = BarcodeSymbology.Code128, Quantity = 1 },
            new() { Data = "DEF456", Symbology = BarcodeSymbology.QRCode, Quantity = 2 }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportToText(items, tempFile);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines.Should().HaveCount(2);
            lines[0].Should().Be("ABC123");
            lines[1].Should().Be("DEF456");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportValidationResults_EmptyList_ExportsOnlyHeader()
    {
        // Arrange
        var results = new List<BatchItemValidationResult>();
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportValidationResults(results, tempFile);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines.Should().HaveCount(1);
            lines[0].Should().Contain("Index,Data,Symbology,Quantity,IsValid,ErrorMessage");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportValidationResults_WithResults_ExportsCorrectly()
    {
        // Arrange
        var results = new List<BatchItemValidationResult>
        {
            new()
            {
                Item = new BarcodeItem { Data = "ABC123", Symbology = BarcodeSymbology.Code128, Quantity = 1 },
                IsValid = true,
                ErrorMessage = "",
                Index = 0
            },
            new()
            {
                Item = new BarcodeItem { Data = "", Symbology = BarcodeSymbology.Code128, Quantity = 1 },
                IsValid = false,
                ErrorMessage = "Data cannot be empty",
                Index = 1
            }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportValidationResults(results, tempFile);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines.Should().HaveCount(3);
            lines[1].Should().Contain("ABC123");
            lines[1].Should().Contain("True");
            lines[2].Should().Contain("False");
            lines[2].Should().Contain("Data cannot be empty");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportValidationResults_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var results = new List<BatchItemValidationResult>
        {
            new()
            {
                Item = new BarcodeItem { Data = "ABC,123", Symbology = BarcodeSymbology.Code128, Quantity = 1 },
                IsValid = false,
                ErrorMessage = "Error: \"invalid\"",
                Index = 0
            }
        };
        var tempFile = CreateTempFile();

        try
        {
            // Act
            BatchExportService.ExportValidationResults(results, tempFile);

            // Assert
            var lines = File.ReadAllLines(tempFile);
            lines[1].Should().Contain("\"ABC,123\"");
            lines[1].Should().Contain("\"Error: \"\"invalid\"\"\"");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

