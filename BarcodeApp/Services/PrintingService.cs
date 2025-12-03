using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using BarcodeApp.Models;
using BarcodeLib;
using ZXing;
using ZXing.Common;

namespace BarcodeApp.Services;

public class PrintingService : IPrintingService
{
    private readonly IBarcodeService _barcodeService;

    public PrintingService(IBarcodeService barcodeService)
    {
        _barcodeService = barcodeService;
    }

    public IEnumerable<string> GetAvailablePrinters()
    {
        return PrinterSettings.InstalledPrinters.Cast<string>();
    }

    public async Task PrintAsync(BarcodeItem item, string printerName, LabelLayout layout, int moduleWidth, int height, bool showText)
    {
        await Task.Run(() =>
        {
            var printDocument = new PrintDocument
            {
                PrinterSettings = { PrinterName = printerName }
            };

            printDocument.PrintPage += (sender, e) =>
            {
                var barcodeImage = GenerateBarcodeBitmap(item.Data, item.Symbology, moduleWidth, height, showText);
                if (barcodeImage != null)
                {
                    var graphics = e.Graphics!;
                    var pageBounds = e.PageBounds;
                    
                    // Convert mm to pixels (assuming 96 DPI)
                    var widthPx = (int)(layout.Width * 3.779527559); // mm to pixels at 96 DPI
                    var heightPx = (int)(layout.Height * 3.779527559);

                    var x = (pageBounds.Width - widthPx) / 2;
                    var y = (pageBounds.Height - heightPx) / 2;

                    // Draw the complete barcode image (includes text if showText is true)
                    graphics.DrawImage(barcodeImage, x, y, widthPx, heightPx);
                }
            };

            printDocument.Print();
        });
    }

    public event EventHandler<PrintProgressEventArgs>? PrintProgress;

    public async Task PrintBatchAsync(IEnumerable<BarcodeItem> items, string printerName, LabelLayout layout, int moduleWidth, int height, bool showText, CancellationToken cancellationToken = default)
    {
        await PrintBatchAsync(items, printerName, layout, moduleWidth, height, showText, 0, int.MaxValue, false, cancellationToken);
    }
    
    public async Task PrintBatchAsync(IEnumerable<BarcodeItem> items, string printerName, LabelLayout layout, int moduleWidth, int height, bool showText, int startIndex, int endIndex, bool continueOnError, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            var printDocument = new PrintDocument
            {
                PrinterSettings = { PrinterName = printerName }
            };

            var itemsList = items.ToList();
            var totalItems = itemsList.Count;
            
            // Apply range
            var start = Math.Max(0, Math.Min(startIndex, totalItems - 1));
            var end = Math.Min(endIndex, totalItems - 1);
            var rangeItems = itemsList.Skip(start).Take(end - start + 1).ToList();
            var rangeTotal = rangeItems.Count;
            
            // Pre-generate all barcode images for better performance
            var preGeneratedImages = new List<Bitmap?>();
            OnPrintProgress(new PrintProgressEventArgs
            {
                CurrentItem = 0,
                TotalItems = rangeTotal,
                Status = "Generating barcode images..."
            });
            
            for (int i = 0; i < rangeItems.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = rangeItems[i];
                var itemModuleWidth = item.ModuleWidth ?? moduleWidth;
                var itemHeight = item.BarcodeHeight ?? height;
                var itemShowText = item.ShowText ?? showText;
                
                var image = GenerateBarcodeBitmap(item.Data, item.Symbology, itemModuleWidth, itemHeight, itemShowText);
                preGeneratedImages.Add(image);
                
                OnPrintProgress(new PrintProgressEventArgs
                {
                    CurrentItem = i + 1,
                    TotalItems = rangeTotal,
                    CurrentItemData = item.Data,
                    Status = $"Generated {i + 1} of {rangeTotal} images"
                });
            }
            
            var currentIndex = 0;
            var failedItems = new List<(int Index, string Data, string Error)>();

            printDocument.PrintPage += (sender, e) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (currentIndex >= rangeItems.Count)
                {
                    e.HasMorePages = false;
                    return;
                }

                var item = rangeItems[currentIndex];
                var quantity = item.Quantity;
                var actualIndex = start + currentIndex;

                // Report progress
                OnPrintProgress(new PrintProgressEventArgs
                {
                    CurrentItem = currentIndex + 1,
                    TotalItems = rangeTotal,
                    CurrentItemData = item.Data,
                    Status = $"Printing item {actualIndex + 1} of {totalItems} (range {start + 1}-{end + 1})"
                });

                try
                {
                    var barcodeImage = currentIndex < preGeneratedImages.Count ? preGeneratedImages[currentIndex] : null;
                    
                    if (barcodeImage == null)
                    {
                        throw new Exception("Failed to generate barcode image");
                    }
                    
                    for (int i = 0; i < quantity && currentIndex < rangeItems.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var graphics = e.Graphics!;
                        var pageBounds = e.PageBounds;
                        var widthPx = (int)(layout.Width * 3.779527559);
                        var heightPx = (int)(layout.Height * 3.779527559);

                        var x = (pageBounds.Width - widthPx) / 2;
                        var y = (pageBounds.Height - heightPx) / 2;

                        // Draw the pre-generated barcode image
                        graphics.DrawImage(barcodeImage, x, y, widthPx, heightPx);
                    }
                }
                catch (Exception ex)
                {
                    failedItems.Add((actualIndex, item.Data, ex.Message));
                    if (!continueOnError)
                    {
                        // Dispose pre-generated images before throwing
                        foreach (var img in preGeneratedImages)
                        {
                            img?.Dispose();
                        }
                        throw;
                    }
                }

                currentIndex++;
                e.HasMorePages = currentIndex < rangeItems.Count;
            };

            try
            {
                printDocument.Print();
            }
            catch (OperationCanceledException)
            {
                OnPrintProgress(new PrintProgressEventArgs
                {
                    CurrentItem = currentIndex,
                    TotalItems = totalItems,
                    Status = "Print cancelled"
                });
                // Dispose images on cancel
                foreach (var img in preGeneratedImages)
                {
                    img?.Dispose();
                }
                throw;
            }
            finally
            {
                // Dispose pre-generated images after printing
                foreach (var img in preGeneratedImages)
                {
                    img?.Dispose();
                }
            }
        }, cancellationToken);
    }

    protected virtual void OnPrintProgress(PrintProgressEventArgs e)
    {
        PrintProgress?.Invoke(this, e);
    }

    private Bitmap? GenerateBarcodeBitmap(string data, BarcodeSymbology symbology, int moduleWidth, int height, bool showText)
    {
        try
        {
            // For 1D barcodes, use BarcodeLib with built-in label support.
            if (!Is2DBarcode(symbology))
            {
                var type = symbology switch
                {
                    BarcodeSymbology.Code128 => TYPE.CODE128,
                    BarcodeSymbology.EAN13 => TYPE.EAN13,
                    BarcodeSymbology.Code39 => TYPE.CODE39,
                    _ => TYPE.CODE128
                };

                var barcode = new Barcode
                {
                    IncludeLabel = showText,
                    LabelPosition = LabelPositions.BOTTOMCENTER,
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    Alignment = AlignmentPositions.CENTER
                };

                var targetWidth = moduleWidth * Math.Max(data.Length, 50);
                var targetHeight = height;

                // BarcodeLib returns a Bitmap already; caller will dispose after printing
                return barcode.Encode(type, data, barcode.ForeColor, barcode.BackColor, targetWidth, targetHeight) as Bitmap;
            }

            // For 2D barcodes (QR / DataMatrix), keep using ZXing
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = GetBarcodeFormat(symbology),
                Options = new EncodingOptions
                {
                    Height = height,
                    Width = moduleWidth * Math.Max(data.Length, 50),
                    Margin = 2,
                    PureBarcode = true
                }
            };

            return writer.Write(data);
        }
        catch
        {
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

    private bool Is2DBarcode(BarcodeSymbology symbology)
    {
        return symbology == BarcodeSymbology.QRCode || symbology == BarcodeSymbology.DataMatrix;
    }
}

