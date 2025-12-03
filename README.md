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