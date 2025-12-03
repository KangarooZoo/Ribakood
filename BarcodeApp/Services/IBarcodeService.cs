using System.Windows.Media;
using BarcodeApp.Models;

namespace BarcodeApp.Services;

public interface IBarcodeService
{
    ImageSource? GenerateBarcode(BarcodeItem item, int moduleWidth, int height, bool showText);
    ImageSource? GenerateBarcode(string data, BarcodeSymbology symbology, int moduleWidth, int height, bool showText);
}

