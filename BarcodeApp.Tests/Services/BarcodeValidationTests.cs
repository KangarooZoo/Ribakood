using BarcodeApp.Models;
using BarcodeApp.Services;
using FluentAssertions;
using Xunit;

namespace BarcodeApp.Tests.Services;

public class BarcodeValidationTests
{
    [Theory]
    [InlineData("", false, "Data cannot be empty")]
    [InlineData("   ", false, "Data cannot be empty")]
    [InlineData(null, false, "Data cannot be empty")]
    public void Validate_EmptyOrWhitespace_ReturnsInvalid(string? data, bool expectedValid, string expectedMessage)
    {
        // Arrange
        var symbology = BarcodeSymbology.Code128;

        // Act
        var result = BarcodeValidation.Validate(data ?? string.Empty, symbology);

        // Assert
        result.IsValid.Should().Be(expectedValid);
        result.Message.Should().Contain(expectedMessage);
    }

    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("1234567890", true)]
    [InlineData("Test-Data_123", true)]
    [InlineData("A", true)]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", true)] // 80 chars
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", false)] // 81 chars
    public void Validate_Code128_ValidatesCorrectly(string data, bool expectedValid)
    {
        // Act
        var result = BarcodeValidation.Validate(data, BarcodeSymbology.Code128);

        // Assert
        result.IsValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            result.Message.Should().Contain("1-80 characters");
        }
    }

    [Theory]
    [InlineData("123456789012", true)] // 12 digits
    [InlineData("1234567890123", true)] // 13 digits
    [InlineData("12345678901", false)] // 11 digits
    [InlineData("12345678901234", false)] // 14 digits
    [InlineData("ABC123456789", false)] // Contains letters
    [InlineData("123456789012a", false)] // Contains letter
    public void Validate_EAN13_ValidatesCorrectly(string data, bool expectedValid)
    {
        // Act
        var result = BarcodeValidation.Validate(data, BarcodeSymbology.EAN13);

        // Assert
        result.IsValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            result.Message.Should().Contain("12 or 13 digits");
        }
    }

    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("TEST-CODE", true)]
    [InlineData("CODE 39", true)]
    [InlineData("CODE$39", true)]
    [InlineData("CODE/39", true)]
    [InlineData("CODE+39", true)]
    [InlineData("CODE%39", true)]
    [InlineData("code39", false)] // Code39 is case-sensitive, only uppercase A-Z
    [InlineData("CODE@39", false)] // Invalid character
    [InlineData("CODE#39", false)] // Invalid character
    [InlineData("", false)]
    public void Validate_Code39_ValidatesCorrectly(string data, bool expectedValid)
    {
        // Act
        var result = BarcodeValidation.Validate(data, BarcodeSymbology.Code39);

        // Assert
        result.IsValid.Should().Be(expectedValid);
        if (!expectedValid && !string.IsNullOrEmpty(data))
        {
            result.Message.Should().Contain("0-9, A-Z");
        }
    }

    [Fact]
    public void Validate_QRCode_ValidatesCorrectly()
    {
        // Test with various lengths
        var result1 = BarcodeValidation.Validate("QR Code Data", BarcodeSymbology.QRCode);
        result1.IsValid.Should().BeTrue();
        
        var result2 = BarcodeValidation.Validate("A", BarcodeSymbology.QRCode);
        result2.IsValid.Should().BeTrue();
        
        var result3 = BarcodeValidation.Validate(new string('A', 2953), BarcodeSymbology.QRCode);
        result3.IsValid.Should().BeTrue();
        
        var result4 = BarcodeValidation.Validate(new string('A', 2954), BarcodeSymbology.QRCode);
        result4.IsValid.Should().BeFalse();
        result4.Message.Should().Contain("too long");
        
        var result5 = BarcodeValidation.Validate("", BarcodeSymbology.QRCode);
        result5.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_DataMatrix_ValidatesCorrectly()
    {
        // Test with various lengths
        var result1 = BarcodeValidation.Validate("Data Matrix Data", BarcodeSymbology.DataMatrix);
        result1.IsValid.Should().BeTrue();
        
        var result2 = BarcodeValidation.Validate("A", BarcodeSymbology.DataMatrix);
        result2.IsValid.Should().BeTrue();
        
        var result3 = BarcodeValidation.Validate(new string('A', 2335), BarcodeSymbology.DataMatrix);
        result3.IsValid.Should().BeTrue();
        
        var result4 = BarcodeValidation.Validate(new string('A', 2336), BarcodeSymbology.DataMatrix);
        result4.IsValid.Should().BeFalse();
        result4.Message.Should().Contain("too long");
        
        var result5 = BarcodeValidation.Validate("", BarcodeSymbology.DataMatrix);
        result5.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_UnknownSymbology_ReturnsInvalid()
    {
        // Arrange
        var data = "TEST";
        var symbology = (BarcodeSymbology)999; // Invalid enum value

        // Act
        var result = BarcodeValidation.Validate(data, symbology);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("Unknown symbology");
    }

    [Fact]
    public void ValidateBatch_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        var items = new List<BarcodeItem>();

        // Act
        var results = BarcodeValidation.ValidateBatch(items);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ValidateBatch_ValidItems_ReturnsAllValid()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "ABC123", Symbology = BarcodeSymbology.Code128 },
            new() { Data = "123456789012", Symbology = BarcodeSymbology.EAN13 },
            new() { Data = "QRCODE", Symbology = BarcodeSymbology.QRCode }
        };

        // Act
        var results = BarcodeValidation.ValidateBatch(items);

        // Assert
        results.Should().HaveCount(3);
        results[0].IsValid.Should().BeTrue();
        results[0].Index.Should().Be(0);
        results[1].IsValid.Should().BeTrue();
        results[1].Index.Should().Be(1);
        results[2].IsValid.Should().BeTrue();
        results[2].Index.Should().Be(2);
    }

    [Fact]
    public void ValidateBatch_InvalidItems_ReturnsAllInvalid()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "", Symbology = BarcodeSymbology.Code128 },
            new() { Data = "123", Symbology = BarcodeSymbology.EAN13 },
            new() { Data = "TEST@CODE", Symbology = BarcodeSymbology.Code39 }
        };

        // Act
        var results = BarcodeValidation.ValidateBatch(items);

        // Assert
        results.Should().HaveCount(3);
        results[0].IsValid.Should().BeFalse();
        results[0].ErrorMessage.Should().NotBeEmpty();
        results[1].IsValid.Should().BeFalse();
        results[1].ErrorMessage.Should().NotBeEmpty();
        results[2].IsValid.Should().BeFalse();
        results[2].ErrorMessage.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateBatch_MixedItems_ReturnsCorrectResults()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "VALID", Symbology = BarcodeSymbology.Code128 },
            new() { Data = "", Symbology = BarcodeSymbology.Code128 },
            new() { Data = "VALID2", Symbology = BarcodeSymbology.Code128 }
        };

        // Act
        var results = BarcodeValidation.ValidateBatch(items);

        // Assert
        results.Should().HaveCount(3);
        results[0].IsValid.Should().BeTrue();
        results[0].ErrorMessage.Should().BeEmpty();
        results[1].IsValid.Should().BeFalse();
        results[1].ErrorMessage.Should().NotBeEmpty();
        results[2].IsValid.Should().BeTrue();
        results[2].ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void ValidateBatch_IndexesAreCorrect()
    {
        // Arrange
        var items = new List<BarcodeItem>
        {
            new() { Data = "ITEM1", Symbology = BarcodeSymbology.Code128 },
            new() { Data = "ITEM2", Symbology = BarcodeSymbology.Code128 },
            new() { Data = "ITEM3", Symbology = BarcodeSymbology.Code128 }
        };

        // Act
        var results = BarcodeValidation.ValidateBatch(items);

        // Assert
        results[0].Index.Should().Be(0);
        results[1].Index.Should().Be(1);
        results[2].Index.Should().Be(2);
    }
}

