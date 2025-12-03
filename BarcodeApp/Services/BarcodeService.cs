using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BarcodeApp.Models;
using BarcodeLib;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace BarcodeApp.Services;

public class BarcodeService : IBarcodeService
{
    public ImageSource? GenerateBarcode(BarcodeItem item, int moduleWidth, int height, bool showText)
    {
        return GenerateBarcode(item.Data, item.Symbology, moduleWidth, height, showText);
    }

    public ImageSource? GenerateBarcode(string data, BarcodeSymbology symbology, int moduleWidth, int height, bool showText)
    {
        try
        {
            // Get actual display DPI for high-resolution rendering
            var dpiX = 96.0;
            var dpiY = 96.0;
            
            // Try to get DPI from main window if available (most accurate)
            if (Application.Current?.MainWindow != null)
            {
                var presentationSource = PresentationSource.FromVisual(Application.Current.MainWindow);
                if (presentationSource?.CompositionTarget != null)
                {
                    var transform = presentationSource.CompositionTarget.TransformToDevice;
                    dpiX = 96.0 * transform.M11;
                    dpiY = 96.0 * transform.M22;
                }
            }
            
            // Fallback to system DPI if window not available
            if (dpiX == 96.0 && dpiY == 96.0)
            {
                try
                {
                    using (var g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        dpiX = g.DpiX;
                        dpiY = g.DpiY;
                    }
                }
                catch
                {
                    // If DPI detection fails, use default 96 DPI
                    dpiX = 96.0;
                    dpiY = 96.0;
                }
            }

            // For 1D barcodes (Code128, EAN13, Code39), use BarcodeLib which has built-in
            // high-quality label (human-readable text) rendering. We keep ZXing for 2D codes.
            if (symbology is BarcodeSymbology.Code128 or BarcodeSymbology.EAN13 or BarcodeSymbology.Code39)
            {
                return GenerateWithBarcodeLib(data, symbology, moduleWidth, height, showText);
            }

            // Calculate DPI scale factor (e.g., 192 DPI = 2.0x scale)
            var dpiScaleX = dpiX / 96.0;
            var dpiScaleY = dpiY / 96.0;
            
            // For vector-like accuracy, generate at high resolution
            // Use consistent scale factor for both with and without text
            var minScaleFactor = 3.0;
            var finalScaleX = Math.Max(dpiScaleX, minScaleFactor);
            var finalScaleY = Math.Max(dpiScaleY, minScaleFactor);
            
            var targetHeight = (int)Math.Round(height * finalScaleY);
            var targetWidth = (int)Math.Round(moduleWidth * (data.Length > 0 ? Math.Max(data.Length, 50) : 50) * finalScaleX);

            // Check if this is a 2D barcode (QR Code, Data Matrix) - these don't need text
            var is2DBarcode = symbology == BarcodeSymbology.QRCode || symbology == BarcodeSymbology.DataMatrix;
            var shouldShowText = showText && !is2DBarcode;

            // Always generate pure barcode (without embedded text) for crisp rendering
            var writer = new BarcodeWriter
            {
                Format = GetBarcodeFormat(symbology),
                Options = new EncodingOptions
                {
                    Height = targetHeight,
                    Width = targetWidth,
                    Margin = 2,
                    PureBarcode = true  // Always pure barcode - we'll add text manually
                }
            };

            var bitmap = writer.Write(data);
            
            // Check if barcode generation succeeded
            if (bitmap == null)
            {
                System.Diagnostics.Debug.WriteLine($"Barcode generation failed: writer.Write returned null for data='{data}', symbology={symbology}");
                return null;
            }

            // Validate bitmap dimensions
            if (bitmap.Width <= 0 || bitmap.Height <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"Barcode generation failed: Invalid bitmap dimensions (Width={bitmap.Width}, Height={bitmap.Height})");
                bitmap.Dispose();
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Bitmap generated: {bitmap.Width}x{bitmap.Height} at DPI {dpiX}x{dpiY}");

            // Convert System.Drawing.Bitmap to high-quality WPF ImageSource
            using (bitmap)
            {
                // Calculate display size for the barcode
                var barcodeDisplayWidth = (int)Math.Round(bitmap.Width / finalScaleX);
                var barcodeDisplayHeight = (int)Math.Round(bitmap.Height / finalScaleY);
                
                // Calculate text area height if needed (scaled to DPI)
                var textHeight = shouldShowText ? (int)Math.Round(30 * dpiScaleY) : 0;
                var spacing = shouldShowText ? (int)Math.Round(8 * dpiScaleY) : 0;
                
                // Create final bitmap with space for text if needed
                var finalHeight = barcodeDisplayHeight + textHeight + spacing;
                var highQualityBitmap = new Bitmap(barcodeDisplayWidth, finalHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
                using (var graphics = Graphics.FromImage(highQualityBitmap))
                {
                    // Set high-quality rendering for the entire image
                    graphics.CompositingMode = CompositingMode.SourceOver;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    
                    // Draw barcode with nearest neighbor interpolation to keep bars crisp
                    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    graphics.SmoothingMode = SmoothingMode.None;
                    
                    // Draw the pure barcode at the top
                    graphics.DrawImage(bitmap, 0, 0, barcodeDisplayWidth, barcodeDisplayHeight);
                    
                    // Draw human-readable text below the barcode if needed
                    if (shouldShowText)
                    {
                        // Switch to high-quality text rendering
                        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        
                        // Calculate font size scaled to DPI (base size 12pt)
                        var fontSize = 12.0f * (float)dpiScaleY;
                        using (var font = new System.Drawing.Font("Arial", fontSize, System.Drawing.FontStyle.Regular, GraphicsUnit.Point))
                        using (var brush = new SolidBrush(System.Drawing.Color.Black))
                        {
                            // Measure text to center it
                            var textSize = graphics.MeasureString(data, font);
                            var textX = (barcodeDisplayWidth - textSize.Width) / 2;
                            var textY = barcodeDisplayHeight + spacing;
                            
                            // Draw the text
                            graphics.DrawString(data, font, brush, textX, textY);
                        }
                    }
                }

                // Lock bitmap data for direct pixel access
                var bitmapData = highQualityBitmap.LockBits(
                    new Rectangle(0, 0, highQualityBitmap.Width, highQualityBitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                try
                {
                    // Calculate buffer size
                    var bufferSize = bitmapData.Stride * bitmapData.Height;
                    var pixelBuffer = new byte[bufferSize];
                    
                    // Copy pixel data to managed buffer
                    Marshal.Copy(bitmapData.Scan0, pixelBuffer, 0, bufferSize);

                    // Set DPI to match the display DPI for proper scaling
                    // This ensures WPF displays the image at the correct size with crisp rendering
                    var displayDpiX = dpiX;
                    var displayDpiY = dpiY;
                    
                    // Create WriteableBitmap with exact DPI settings for crisp rendering
                    var writeableBitmap = new WriteableBitmap(
                        bitmapData.Width,
                        bitmapData.Height,
                        displayDpiX,
                        displayDpiY,
                        System.Windows.Media.PixelFormats.Bgra32,
                        null);

                    // Write pixels directly to WriteableBitmap (no interpolation)
                    writeableBitmap.WritePixels(
                        new Int32Rect(0, 0, bitmapData.Width, bitmapData.Height),
                        pixelBuffer,
                        bitmapData.Stride,
                        0);

                    // Freeze for thread safety and performance
                    writeableBitmap.Freeze();

                    System.Diagnostics.Debug.WriteLine($"High-DPI BitmapSource created: {writeableBitmap.Width}x{writeableBitmap.Height} at {writeableBitmap.DpiX}x{writeableBitmap.DpiY} DPI");

                    return writeableBitmap;
                }
                finally
                {
                    highQualityBitmap.UnlockBits(bitmapData);
                    highQualityBitmap.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            // Log detailed error information for debugging
            System.Diagnostics.Debug.WriteLine($"Barcode generation error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Data: '{data}', Symbology: {symbology}, ModuleWidth: {moduleWidth}, Height: {height}, ShowText: {showText}");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    private ImageSource? GenerateWithBarcodeLib(string data, BarcodeSymbology symbology, int moduleWidth, int height, bool showText)
    {
        try
        {
            var type = symbology switch
            {
                BarcodeSymbology.Code128 => TYPE.CODE128,
                BarcodeSymbology.EAN13 => TYPE.EAN13,
                BarcodeSymbology.Code39 => TYPE.CODE39,
                _ => TYPE.CODE128
            };

            // Configure BarcodeLib
            var barcode = new Barcode
            {
                IncludeLabel = showText,
                LabelPosition = LabelPositions.BOTTOMCENTER,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.Black,
                Alignment = AlignmentPositions.CENTER
            };

            // Width based on moduleWidth and data length (similar to ZXing calculation)
            var targetWidth = moduleWidth * (data.Length > 0 ? Math.Max(data.Length, 50) : 50);
            var targetHeight = height;

            using (var image = barcode.Encode(type, data, barcode.ForeColor, barcode.BackColor, targetWidth, targetHeight))
            {
                if (image is not Bitmap bitmap)
                {
                    return null;
                }

                // Convert Bitmap to WPF ImageSource via WriteableBitmap for crisp display
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                try
                {
                    var bufferSize = bitmapData.Stride * bitmapData.Height;
                    var pixelBuffer = new byte[bufferSize];
                    Marshal.Copy(bitmapData.Scan0, pixelBuffer, 0, bufferSize);

                    var writeableBitmap = new WriteableBitmap(
                        bitmapData.Width,
                        bitmapData.Height,
                        96,
                        96,
                        System.Windows.Media.PixelFormats.Bgra32,
                        null);

                    writeableBitmap.WritePixels(
                        new Int32Rect(0, 0, bitmapData.Width, bitmapData.Height),
                        pixelBuffer,
                        bitmapData.Stride,
                        0);

                    writeableBitmap.Freeze();
                    return writeableBitmap;
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BarcodeLib generation error: {ex.Message}");
            return null;
        }
    }

    private BarcodeFormat GetBarcodeFormat(BarcodeSymbology symbology)
    {
        return symbology switch
        {
            BarcodeSymbology.Code128 => BarcodeFormat.CODE_128,
            BarcodeSymbology.EAN13 => BarcodeFormat.EAN_13,
            BarcodeSymbology.Code39 => BarcodeFormat.CODE_39,
            BarcodeSymbology.QRCode => BarcodeFormat.QR_CODE,
            BarcodeSymbology.DataMatrix => BarcodeFormat.DATA_MATRIX,
            _ => BarcodeFormat.CODE_128
        };
    }
}

