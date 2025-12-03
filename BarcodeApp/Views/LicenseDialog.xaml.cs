using System;
using System.Windows;
using System.Windows.Media;
using BarcodeApp.ViewModels;

namespace BarcodeApp.Views;

public partial class LicenseDialog : Window
{
    public LicenseDialog(LicenseViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Apply light theme to this window's resources
        ApplyLightTheme();
        
        viewModel.ActivationCompleted += OnActivationCompleted;
        viewModel.CloseRequested += (sender, e) =>
        {
            if (CheckAccess())
            {
                Close();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => Close()));
            }
        };
    }

    private void OnActivationCompleted(object? sender, bool success)
    {
        if (!success) return;

        // Ensure we're on the UI thread and window is ready
        if (CheckAccess())
        {
            CloseDialog();
        }
        else
        {
            // Use BeginInvoke with lower priority to ensure window is fully shown
            Dispatcher.BeginInvoke(new Action(CloseDialog), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void CloseDialog()
    {
        try
        {
            // Try to set DialogResult - this will only work if window was shown via ShowDialog()
            // and is in a valid state. If it fails, the exception will be caught.
            DialogResult = true;
        }
        catch (InvalidOperationException)
        {
            // DialogResult can't be set - window might not be shown as dialog yet
            // This is okay, we'll just close without setting the result
        }
        finally
        {
            // Always close the window, regardless of whether DialogResult was set
            Close();
        }
    }
    
    private void ApplyLightTheme()
    {
        if (Resources != null)
        {
            // Apply light theme colors
            Resources["MaterialDesignPaper"] = new SolidColorBrush(Colors.White);
            Resources["MaterialDesignBody"] = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21)); // Dark text on light bg
            Resources["MaterialDesignDivider"] = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)); // Darker divider for visibility
            Resources["MaterialDesignBackground"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5)); // Slightly darker background
            Resources["MaterialDesignSecondaryTextBrush"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42)); // Much darker for better contrast
        }
    }
}

