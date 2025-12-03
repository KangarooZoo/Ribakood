using System.Windows;
using System.Windows.Input;
using BarcodeApp.ViewModels;

namespace BarcodeApp.Views;

public partial class LicenseModal
{
    public LicenseModal()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LicenseViewModel viewModel)
        {
            if (viewModel.ActivateCommand.CanExecute(null))
            {
                viewModel.ActivateCommand.Execute(null);
            }
        }
    }
}

