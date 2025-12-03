# BarcodeApp Tests

This directory contains comprehensive unit tests for the BarcodeApp application.

## Test Coverage

The test suite covers:

### Services
- **BarcodeValidationTests**: Tests for barcode data validation across all symbologies (Code128, EAN13, Code39, QRCode, DataMatrix)
- **BarcodeServiceTests**: Tests for barcode image generation
- **CsvParserTests**: Tests for CSV file parsing and batch import functionality
- **BatchExportServiceTests**: Tests for exporting barcode data to CSV and text files
- **MachineIdProviderTests**: Tests for machine ID generation and consistency

### Converters
- **BooleanToVisibilityConverterTests**: Tests for boolean to visibility conversion
- **InverseBooleanToVisibilityConverterTests**: Tests for inverse boolean to visibility conversion
- **InverseBooleanConverterTests**: Tests for boolean inversion
- **ObjectToVisibilityConverterTests**: Tests for object to visibility conversion
- **StringToVisibilityConverterTests**: Tests for string to visibility conversion
- **BooleanToBrushConverterTests**: Tests for boolean to brush color conversion
- **SliderValueToWidthConverterTests**: Tests for slider value to width calculation

### Models
- **BarcodeItemTests**: Tests for barcode item model
- **BarcodeSymbologyTests**: Tests for symbology enum
- **BatchItemValidationResultTests**: Tests for validation result model
- **BatchPrintOptionsTests**: Tests for batch print options model
- **LabelLayoutTests**: Tests for label layout model
- **LicenseTokenTests**: Tests for license token model
- **PrintProgressTests**: Tests for print progress model

## Running Tests

### Using Visual Studio
1. Open the solution in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Open Test Explorer (Test → Test Explorer)
4. Click "Run All" or run individual test classes

### Using .NET CLI
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage (requires coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Using Visual Studio Code
1. Install the .NET Core Test Explorer extension
2. Open the Command Palette (Ctrl+Shift+P)
3. Run "Test: Run All Tests"

## Test Structure

Tests follow the Arrange-Act-Assert (AAA) pattern:
- **Arrange**: Set up test data and conditions
- **Act**: Execute the code under test
- **Assert**: Verify the expected outcomes

## Dependencies

- **xUnit**: Testing framework
- **FluentAssertions**: Fluent assertion library for readable test assertions
- **Moq**: Mocking framework (available for future use with service mocking)

## Notes

- Some tests that interact with WPF (like BarcodeService) may require WPF context
- File I/O tests use temporary files that are cleaned up after execution
- Machine ID tests verify consistency but actual values depend on the machine

