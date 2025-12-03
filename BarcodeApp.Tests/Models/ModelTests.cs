using BarcodeApp.Models;
using FluentAssertions;
using Xunit;

namespace BarcodeApp.Tests.Models;

public class BarcodeItemTests
{
    [Fact]
    public void BarcodeItem_DefaultConstructor_SetsDefaults()
    {
        // Act
        var item = new BarcodeItem();

        // Assert
        item.Data.Should().BeEmpty();
        item.Symbology.Should().Be(BarcodeSymbology.Code128);
        item.Quantity.Should().Be(1);
        item.ModuleWidth.Should().BeNull();
        item.BarcodeHeight.Should().BeNull();
        item.ShowText.Should().BeNull();
    }

    [Fact]
    public void BarcodeItem_CanSetAllProperties()
    {
        // Arrange & Act
        var item = new BarcodeItem
        {
            Data = "TEST123",
            Symbology = BarcodeSymbology.QRCode,
            Quantity = 5,
            ModuleWidth = 3,
            BarcodeHeight = 150,
            ShowText = true
        };

        // Assert
        item.Data.Should().Be("TEST123");
        item.Symbology.Should().Be(BarcodeSymbology.QRCode);
        item.Quantity.Should().Be(5);
        item.ModuleWidth.Should().Be(3);
        item.BarcodeHeight.Should().Be(150);
        item.ShowText.Should().Be(true);
    }
}

public class BarcodeSymbologyTests
{
    [Fact]
    public void BarcodeSymbology_AllValues_AreDefined()
    {
        // Assert
        Enum.GetValues<BarcodeSymbology>().Should().HaveCount(5);
        Enum.GetValues<BarcodeSymbology>().Should().Contain(BarcodeSymbology.Code128);
        Enum.GetValues<BarcodeSymbology>().Should().Contain(BarcodeSymbology.EAN13);
        Enum.GetValues<BarcodeSymbology>().Should().Contain(BarcodeSymbology.Code39);
        Enum.GetValues<BarcodeSymbology>().Should().Contain(BarcodeSymbology.QRCode);
        Enum.GetValues<BarcodeSymbology>().Should().Contain(BarcodeSymbology.DataMatrix);
    }
}

public class BatchItemValidationResultTests
{
    [Fact]
    public void BatchItemValidationResult_DefaultConstructor_SetsDefaults()
    {
        // Act
        var result = new BatchItemValidationResult();

        // Assert
        result.Item.Should().BeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().BeEmpty();
        result.Index.Should().Be(0);
    }

    [Fact]
    public void BatchItemValidationResult_CanSetAllProperties()
    {
        // Arrange
        var item = new BarcodeItem { Data = "TEST", Symbology = BarcodeSymbology.Code128 };

        // Act
        var result = new BatchItemValidationResult
        {
            Item = item,
            IsValid = true,
            ErrorMessage = "No errors",
            Index = 5
        };

        // Assert
        result.Item.Should().Be(item);
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().Be("No errors");
        result.Index.Should().Be(5);
    }
}

public class BatchPrintOptionsTests
{
    [Fact]
    public void BatchPrintOptions_DefaultConstructor_SetsDefaults()
    {
        // Act
        var options = new BatchPrintOptions();

        // Assert
        options.StartIndex.Should().Be(0);
        options.EndIndex.Should().Be(int.MaxValue);
        options.ContinueOnError.Should().BeFalse();
        options.SkipPrintedItems.Should().BeFalse();
    }

    [Fact]
    public void BatchPrintOptions_CanSetAllProperties()
    {
        // Act
        var options = new BatchPrintOptions
        {
            StartIndex = 10,
            EndIndex = 50,
            ContinueOnError = true,
            SkipPrintedItems = true
        };

        // Assert
        options.StartIndex.Should().Be(10);
        options.EndIndex.Should().Be(50);
        options.ContinueOnError.Should().BeTrue();
        options.SkipPrintedItems.Should().BeTrue();
    }
}

public class LabelLayoutTests
{
    [Fact]
    public void LabelLayout_DefaultConstructor_SetsDefaults()
    {
        // Act
        var layout = new LabelLayout();

        // Assert
        layout.Name.Should().BeEmpty();
        layout.Width.Should().Be(0.0);
        layout.Height.Should().Be(0.0);
    }

    [Fact]
    public void LabelLayout_CanSetAllProperties()
    {
        // Act
        var layout = new LabelLayout
        {
            Name = "A4",
            Width = 210.0,
            Height = 297.0
        };

        // Assert
        layout.Name.Should().Be("A4");
        layout.Width.Should().Be(210.0);
        layout.Height.Should().Be(297.0);
    }
}

public class LicenseTokenTests
{
    [Fact]
    public void LicenseToken_DefaultConstructor_SetsDefaults()
    {
        // Act
        var token = new LicenseToken();

        // Assert
        token.Key.Should().BeEmpty();
        token.MachineId.Should().BeEmpty();
        token.ExpiryDate.Should().Be(default(DateTime));
        token.IssuedAt.Should().Be(default(DateTime));
        token.LastSuccessfulValidation.Should().BeNull();
    }

    [Fact]
    public void LicenseToken_CanSetAllProperties()
    {
        // Arrange
        var expiryDate = DateTime.UtcNow.AddDays(365);
        var issuedAt = DateTime.UtcNow;
        var lastValidation = DateTime.UtcNow;

        // Act
        var token = new LicenseToken
        {
            Key = "TEST-LICENSE-001",
            MachineId = "machine-id-123",
            ExpiryDate = expiryDate,
            IssuedAt = issuedAt,
            LastSuccessfulValidation = lastValidation
        };

        // Assert
        token.Key.Should().Be("TEST-LICENSE-001");
        token.MachineId.Should().Be("machine-id-123");
        token.ExpiryDate.Should().Be(expiryDate);
        token.IssuedAt.Should().Be(issuedAt);
        token.LastSuccessfulValidation.Should().Be(lastValidation);
    }
}

public class PrintProgressTests
{
    [Fact]
    public void PrintProgress_DefaultConstructor_SetsDefaults()
    {
        // Act
        var progress = new PrintProgress();

        // Assert
        progress.CurrentItem.Should().Be(0);
        progress.TotalItems.Should().Be(0);
        progress.CurrentItemData.Should().BeEmpty();
        progress.Status.Should().BeEmpty();
        progress.ProgressPercentage.Should().Be(0.0);
    }

    [Fact]
    public void PrintProgress_CanSetAllProperties()
    {
        // Act
        var progress = new PrintProgress
        {
            CurrentItem = 5,
            TotalItems = 10,
            CurrentItemData = "ABC123",
            Status = "Printing..."
        };

        // Assert
        progress.CurrentItem.Should().Be(5);
        progress.TotalItems.Should().Be(10);
        progress.CurrentItemData.Should().Be("ABC123");
        progress.Status.Should().Be("Printing...");
    }

    [Fact]
    public void PrintProgress_ProgressPercentage_CalculatesCorrectly()
    {
        // Arrange
        var progress = new PrintProgress
        {
            CurrentItem = 5,
            TotalItems = 10
        };

        // Act & Assert
        progress.ProgressPercentage.Should().Be(50.0);
    }

    [Fact]
    public void PrintProgress_ProgressPercentage_ZeroTotal_ReturnsZero()
    {
        // Arrange
        var progress = new PrintProgress
        {
            CurrentItem = 5,
            TotalItems = 0
        };

        // Act & Assert
        progress.ProgressPercentage.Should().Be(0.0);
    }

    [Fact]
    public void PrintProgress_ProgressPercentage_Complete_Returns100()
    {
        // Arrange
        var progress = new PrintProgress
        {
            CurrentItem = 10,
            TotalItems = 10
        };

        // Act & Assert
        progress.ProgressPercentage.Should().Be(100.0);
    }
}

