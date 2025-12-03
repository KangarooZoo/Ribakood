using System;
using System.Globalization;
using System.Windows;
using BarcodeApp.Converters;
using FluentAssertions;
using Xunit;

namespace BarcodeApp.Tests.Converters;

public class BooleanToVisibilityConverterTests
{
    private readonly BooleanToVisibilityConverter _converter;

    public BooleanToVisibilityConverterTests()
    {
        _converter = new BooleanToVisibilityConverter();
    }

    [Fact]
    public void Convert_True_ReturnsVisible()
    {
        // Act
        var result = _converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_False_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonBoolean_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert("not a bool", typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_Visible_ReturnsTrue()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void ConvertBack_Collapsed_ReturnsFalse()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_Hidden_ReturnsFalse()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(false);
    }
}

public class InverseBooleanToVisibilityConverterTests
{
    private readonly InverseBooleanToVisibilityConverter _converter;

    public InverseBooleanToVisibilityConverterTests()
    {
        _converter = new InverseBooleanToVisibilityConverter();
    }

    [Fact]
    public void Convert_True_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_False_ReturnsVisible()
    {
        // Act
        var result = _converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_NonBoolean_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert("not a bool", typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        // InverseBooleanToVisibilityConverter only inverts boolean values, non-booleans return Collapsed
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_Visible_ReturnsFalse()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_Collapsed_ReturnsTrue()
    {
        // Act
        var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(true);
    }
}

public class BooleanToBrushConverterTests
{
    private readonly BooleanToBrushConverter _converter;

    public BooleanToBrushConverterTests()
    {
        _converter = new BooleanToBrushConverter();
    }

    [Fact]
    public void Convert_True_ReturnsGreenBrush()
    {
        // Act
        var result = _converter.Convert(true, typeof(System.Windows.Media.Brush), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().BeAssignableTo<System.Windows.Media.SolidColorBrush>();
        var brush = result as System.Windows.Media.SolidColorBrush;
        brush!.Color.R.Should().Be(0x38);
        brush.Color.G.Should().Be(0x8E);
        brush.Color.B.Should().Be(0x3C);
    }

    [Fact]
    public void Convert_False_ReturnsRedBrush()
    {
        // Act
        var result = _converter.Convert(false, typeof(System.Windows.Media.Brush), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().BeAssignableTo<System.Windows.Media.SolidColorBrush>();
        var brush = result as System.Windows.Media.SolidColorBrush;
        brush!.Color.R.Should().Be(0xD3);
        brush.Color.G.Should().Be(0x2F);
        brush.Color.B.Should().Be(0x2F);
    }

    [Fact]
    public void Convert_NonBoolean_ReturnsGrayBrush()
    {
        // Act
        var result = _converter.Convert("not a bool", typeof(System.Windows.Media.Brush), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(System.Windows.Media.Brushes.Gray);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            _converter.ConvertBack(null, typeof(bool), null, CultureInfo.CurrentCulture));
    }
}

public class InverseBooleanConverterTests
{
    private readonly InverseBooleanConverter _converter;

    public InverseBooleanConverterTests()
    {
        _converter = new InverseBooleanConverter();
    }

    [Fact]
    public void Convert_True_ReturnsFalse()
    {
        // Act
        var result = _converter.Convert(true, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_False_ReturnsTrue()
    {
        // Act
        var result = _converter.Convert(false, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_NonBoolean_ReturnsTrue()
    {
        // Act
        var result = _converter.Convert("not a bool", typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void ConvertBack_True_ReturnsFalse()
    {
        // Act
        var result = _converter.ConvertBack(true, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_False_ReturnsTrue()
    {
        // Act
        var result = _converter.ConvertBack(false, typeof(bool), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(true);
    }
}

public class ObjectToVisibilityConverterTests
{
    private readonly ObjectToVisibilityConverter _converter;

    public ObjectToVisibilityConverterTests()
    {
        _converter = new ObjectToVisibilityConverter();
    }

    [Fact]
    public void Convert_NonNullObject_ReturnsVisible()
    {
        // Act
        var result = _converter.Convert("test", typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            _converter.ConvertBack(null!, typeof(object), null, CultureInfo.CurrentCulture));
    }
}

public class StringToVisibilityConverterTests
{
    private readonly StringToVisibilityConverter _converter;

    public StringToVisibilityConverterTests()
    {
        _converter = new StringToVisibilityConverter();
    }

    [Fact]
    public void Convert_NonEmptyString_ReturnsVisible()
    {
        // Act
        var result = _converter.Convert("test", typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_EmptyString_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert("", typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_WhitespaceString_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert("   ", typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        // Act
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonStringObject_ConvertsToString()
    {
        // Act
        var result = _converter.Convert(123, typeof(Visibility), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(Visibility.Visible); // 123.ToString() is "123"
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            _converter.ConvertBack(null!, typeof(string), null, CultureInfo.CurrentCulture));
    }
}

public class SliderValueToWidthConverterTests
{
    private readonly SliderValueToWidthConverter _converter;

    public SliderValueToWidthConverterTests()
    {
        _converter = new SliderValueToWidthConverter();
    }

    [Fact]
    public void Convert_ValidValues_CalculatesCorrectly()
    {
        // Arrange
        var values = new object[] { 50.0, 0.0, 100.0, 200.0 }; // value, min, max, trackWidth

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(100.0); // 50% of 200
    }

    [Fact]
    public void Convert_MinimumValue_ReturnsZero()
    {
        // Arrange
        var values = new object[] { 0.0, 0.0, 100.0, 200.0 };

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void Convert_MaximumValue_ReturnsTrackWidth()
    {
        // Arrange
        var values = new object[] { 100.0, 0.0, 100.0, 200.0 };

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(200.0);
    }

    [Fact]
    public void Convert_NullValues_ReturnsZero()
    {
        // Act
        var result = _converter.Convert(null, typeof(double), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void Convert_InsufficientValues_ReturnsZero()
    {
        // Arrange
        var values = new object[] { 50.0, 0.0 }; // Only 2 values, need 4

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void Convert_MinEqualsMax_ReturnsZero()
    {
        // Arrange
        var values = new object[] { 50.0, 100.0, 100.0, 200.0 };

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void Convert_InvalidTypes_ReturnsZero()
    {
        // Arrange
        var values = new object[] { "not a number", 0.0, 100.0, 200.0 };

        // Act
        var result = _converter.Convert(values, typeof(double), null, CultureInfo.CurrentCulture);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            _converter.ConvertBack(null!, new[] { typeof(double) }, null, CultureInfo.CurrentCulture));
    }
}

