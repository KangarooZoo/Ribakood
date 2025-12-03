using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BarcodeApp.Models;
using BarcodeApp.ViewModels;

namespace BarcodeApp;

public partial class MainWindow : Window
{
    public static ObservableCollection<BarcodeSymbology> Symbologies { get; } = new()
    {
        BarcodeSymbology.Code128,
        BarcodeSymbology.EAN13,
        BarcodeSymbology.Code39,
        BarcodeSymbology.QRCode,
        BarcodeSymbology.DataMatrix
    };

    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.MainViewModel;
    }

    private void LicenseBadge_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.ShowLicenseModalCommand.Execute(null);
        }
    }
}