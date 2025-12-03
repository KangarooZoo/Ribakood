using System;
using System.Windows;
using System.Windows.Threading;
using BarcodeApp.Services;
using BarcodeApp.ViewModels;
using BarcodeApp.Views;

namespace BarcodeApp;

public partial class App : Application
{
    public static IThemeService ThemeService { get; private set; } = null!;
    public static IBarcodeService BarcodeService { get; private set; } = null!;
    public static IPrintingService PrintingService { get; private set; } = null!;
    public static ILicensingService LicensingService { get; private set; } = null!;
    public static MainViewModel MainViewModel { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Initialize services
            ThemeService = new ThemeService();
            BarcodeService = new BarcodeService();
            PrintingService = new PrintingService(BarcodeService);
            LicensingService = new LicensingService();

            // Apply saved theme (defaults to dark)
            ThemeService.ApplySavedTheme();

            // Check license status
            var licenseStatus = LicensingService.GetLicenseStatus();

            // Create MainViewModel
            MainViewModel = new MainViewModel(ThemeService, BarcodeService, PrintingService, LicensingService);

            // Temporarily bypass license check to get app running
            // TODO: Re-enable license check once MaterialDesign is working
            /*
            // If license is invalid and not in grace period, show activation dialog
            if (!licenseStatus.IsValid && !licenseStatus.IsGracePeriod)
            {
                var licenseDialog = new LicenseDialog(new LicenseViewModel(LicensingService))
                {
                    Owner = null // Will be set when MainWindow is shown
                };

                var result = licenseDialog.ShowDialog();
                if (result != true)
                {
                    // User didn't activate, exit application
                    Shutdown();
                    return;
                }
            }
            */

            // Show main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Application startup error:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
    
    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
    }
    
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Unhandled exception:\n\n{e.Exception.Message}\n\nStack trace:\n{e.Exception.StackTrace}", 
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
