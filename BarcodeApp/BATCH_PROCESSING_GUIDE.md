# Batch Processing Guide

This guide explains how batch processing works in the Barcode App.

## Overview

Batch processing allows you to load multiple barcode data entries from a file and print them all at once, rather than entering each one manually.

## How It Works

### 1. File Format

The batch file can be a **CSV** or **text file** with a simple format:
- **One barcode per line**
- Each line contains the barcode data (text/number)
- Empty lines are ignored
- Lines are automatically trimmed of whitespace

**Example `test_batch.csv`:**
```
1234567890128
ABC123
PROD-001
ITEM-2024-001
9876543210987
```

### 2. Loading a Batch File

1. Click the **"Load Batch File"** button in the app
2. Select a CSV or text file from your computer
3. The app reads all lines from the file
4. Each non-empty line becomes a `BarcodeItem` with:
   - **Data**: The text from the line (trimmed)
   - **Symbology**: Uses the currently selected symbology (Code128, EAN13, etc.)
   - **Quantity**: Set to 1 for each item

### 3. Batch Mode Activation

When a batch file is loaded:
- `IsBatchMode` is set to `true`
- The batch items are stored in `BatchItems` collection
- A counter shows: **"Batch Items: X"** (where X is the number of items)
- The counter is only visible when batch mode is active

### 4. Printing Batch Items

When you click **"Print Now"** in batch mode:

1. The app checks if you have a valid license
2. It verifies that batch items exist
3. For each item in the batch:
   - Generates a barcode image using the item's data
   - Uses the current settings:
     - Selected symbology
     - Module width (from slider)
     - Barcode height (from slider)
     - Show text option
   - Prints one label per item
   - Each item prints on a separate page/label

### 5. Print Process Details

The batch printing uses a `PrintDocument` with a `PrintPage` event handler:

```csharp
// For each item in the batch:
1. Generate barcode bitmap from item data
2. Calculate label dimensions (converts mm to pixels)
3. Center the barcode on the page
4. Draw the barcode image
5. Move to next item
6. Continue until all items are printed
```

**Key Points:**
- Each barcode is printed on a **separate page/label**
- All items use the **same settings** (symbology, size, etc.)
- The quantity field in `BarcodeItem` is currently set to 1 (not used in batch mode)
- Printing happens asynchronously to avoid blocking the UI

## Code Flow

### Loading Batch File

```csharp
LoadBatchFile() {
  1. Open file dialog (CSV/TXT filter)
  2. Read all lines from file
  3. Clear existing batch items
  4. For each line:
     - Skip if empty/whitespace
     - Create BarcodeItem with:
       * Data = line.Trim()
       * Symbology = SelectedSymbology (current selection)
       * Quantity = 1
     - Add to BatchItems collection
  5. Set IsBatchMode = true if items loaded
}
```

### Printing Batch

```csharp
PrintNow() {
  if (IsBatchMode && BatchItems.Any()) {
    PrintBatchAsync(BatchItems, ...) {
      For each item:
        1. Generate barcode bitmap
        2. Print on separate page
        3. Move to next item
    }
  }
}
```

## File Format Examples

### Simple Text File (`items.txt`)
```
PRODUCT-001
PRODUCT-002
PRODUCT-003
```

### CSV File (`batch.csv`)
```
1234567890128
ABC123
PROD-001
ITEM-2024-001
```

**Note:** Even though it's called CSV, the current implementation treats it as a simple text file (one value per line). It doesn't parse comma-separated values.

## Current Limitations

1. **Single Symbology**: All items in a batch use the same symbology (the one selected when loading)
2. **Quantity = 1**: Each item prints once (the Quantity field isn't used in batch mode)
3. **No Validation**: The app doesn't validate barcode data format before printing
4. **No Preview**: You can't preview individual batch items before printing
5. **No Editing**: Once loaded, you can't edit individual items in the batch
6. **Simple Format**: Only supports one barcode per line (not true CSV with multiple columns)

## Usage Tips

1. **Prepare Your File**: Create a text or CSV file with one barcode per line
2. **Select Symbology First**: Choose the symbology type before loading the batch file
3. **Configure Settings**: Set module width, height, and text visibility before printing
4. **Check Printer**: Make sure your printer is selected and has paper/labels loaded
5. **Test First**: Test with a small batch file (2-3 items) before printing large batches

## Example Workflow

1. **Prepare Data File**:
   ```
   Create: products.txt
   Content:
   PROD-001
   PROD-002
   PROD-003
   ```

2. **In the App**:
   - Select symbology: Code128
   - Adjust module width: 2
   - Adjust height: 100px
   - Enable "Show Human-Readable Text"
   - Select printer: Your Label Printer
   - Select layout: A4 (or your label size)

3. **Load Batch**:
   - Click "Load Batch File"
   - Select `products.txt`
   - See: "Batch Items: 3"

4. **Print**:
   - Click "Print Now"
   - Three labels print, one for each product code

## Future Enhancements (Potential)

- Support for true CSV format with multiple columns
- Per-item symbology selection
- Per-item quantity support
- Batch item validation before printing
- Preview of batch items
- Edit/remove individual batch items
- Export batch items to file
- Batch templates/saved batches

## Troubleshooting

### Batch file not loading
- Check file format (one item per line)
- Ensure file is not empty
- Check file encoding (should be UTF-8 or ASCII)

### Items not printing
- Verify license is valid
- Check printer is selected and online
- Ensure printer has paper/labels
- Check printer queue for errors

### Wrong symbology
- Make sure you select the correct symbology **before** loading the batch file
- All items in a batch use the symbology that was selected when the file was loaded

### Labels not centered
- Check your label layout settings
- Verify printer page size matches layout

