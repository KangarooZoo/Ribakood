# Barcode Generator & Printer Application

A modern, secure desktop application for generating and printing barcodes, built with WPF, Material Design, and the MVVM pattern.

## Overview

This application provides a comprehensive solution for barcode generation and printing with support for multiple barcode formats, batch processing, and secure license management.

## Features

- **Multiple Barcode Formats**: Supports Code 128, EAN-13, Code 39, QR Code, and Data Matrix
- **Live Preview**: Real-time barcode preview with customizable size and appearance
- **Batch Processing**: Load CSV/text files for bulk barcode generation and printing
- **Secure Licensing**: Server-based license validation with DPAPI-encrypted local storage
- **Offline Grace Period**: 7-day grace period for offline operation
- **Material Design UI**: Modern, responsive interface with light/dark theme support
- **Printing**: Support for single and batch printing with customizable label layouts

## Project Structure

```
Ribakood/
├── BarcodeApp/              # Main WPF application
│   ├── Models/              # Data models
│   ├── ViewModels/          # MVVM ViewModels
│   ├── Views/               # XAML views and dialogs
│   ├── Services/            # Business logic services
│   ├── Converters/          # WPF value converters
│   ├── Controls/            # Custom controls
│   ├── Resources/           # Resource dictionaries (Material Design)
│   └── Properties/          # Application settings
├── BarcodeApp.Tests/        # Unit tests
└── README.md                # This file
```

## Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11
- Visual Studio 2022 or VS Code (recommended)

## Getting Started

### Building the Application

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd Ribakood
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   cd BarcodeApp
   dotnet run
   ```

### Running Tests

```bash
cd BarcodeApp.Tests
dotnet test
```

## Configuration

### License Server

The application requires a license validation server endpoint. By default, it's configured to:
- **URL**: `https://v0-license-server-setup.vercel.app/api/validateKey`
- **Method**: POST

To change the license server URL, edit `BarcodeApp/Services/LicensingService.cs` and update the `LicenseServerUrl` constant.

See `BarcodeApp/README.md` for detailed configuration instructions.

## Architecture

The application follows the **MVVM (Model-View-ViewModel)** pattern:

- **Models**: Data structures representing barcodes, labels, and license information
- **ViewModels**: Business logic and state management
- **Views**: XAML-based UI with Material Design styling
- **Services**: Reusable business logic components

### Key Services

- `BarcodeService`: Generates barcode images using ZXing.Net
- `BarcodeValidation`: Validates input data against symbology requirements
- `PrintingService`: Handles printer enumeration and print job execution
- `LicensingService`: Manages license validation, DPAPI encryption, and offline grace period
- `ThemeService`: Manages light/dark theme switching with persistence
- `MachineIdProvider`: Generates SHA-256 hash of machine identifier

## Security Features

- **DPAPI Encryption**: License tokens are encrypted using Windows Data Protection API
- **Machine Binding**: License is bound to machine ID (SHA-256 hash of MAC address)
- **HTTPS Communication**: All license validation requests use secure HTTPS
- **Exponential Backoff**: Network retry logic prevents server overload

## Usage

1. **Generate Single Barcode**:
   - Enter data in the input field
   - Select symbology type
   - Adjust module width and height using sliders
   - Toggle human-readable text visibility
   - Click "Print Now" (requires valid license)

2. **Batch Processing**:
   - Click "Load Batch File"
   - Select a CSV or text file (one barcode per line)
   - Review batch items in the preview
   - Configure print options
   - Click "Print Now" to print all items

3. **Theme Switching**:
   - Use the "Dark Theme" toggle in the top-right corner
   - Preference is saved and restored on next launch

## Dependencies

### Main Application
- MaterialDesignThemes (5.3.0)
- MaterialDesignColors (5.3.0)
- ZXing.Net (0.16.11)
- ZXing.Net.Bindings.Windows.Compatibility (0.16.14)
- BarcodeLib (2.4.0)
- System.Drawing.Common (9.0.10)

## Testing

The project includes comprehensive unit tests covering:
- Barcode generation and validation
- CSV parsing
- Batch export functionality
- License service integration
- Machine ID generation
- Value converters

Run tests with:
```bash
dotnet test
```

## License

This application requires a valid license key for printing functionality. Preview and generation features are available without activation, but printing is restricted until a valid license is activated.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests as needed
5. Submit a pull request

## Support

For detailed documentation, see:
- `BarcodeApp/README.md` - Main application documentation
- `BarcodeApp.Tests/README.md` - Testing documentation

## License

Copyright © 2024. All rights reserved.

