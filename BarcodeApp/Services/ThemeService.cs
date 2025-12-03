using System.Windows;
using System.Windows.Media;

namespace BarcodeApp.Services;

public interface IThemeService
{
    bool IsDarkTheme { get; }
    void ApplySavedTheme();
    void ToggleTheme();
}

public class ThemeService : IThemeService
{
    public bool IsDarkTheme { get; private set; } = true;

    public void ApplySavedTheme()
    {
        var saved = Properties.Settings.Default.Theme;
        if (string.Equals(saved, "Light", StringComparison.OrdinalIgnoreCase))
        {
            SetBaseTheme(false);
        }
        else
        {
            SetBaseTheme(true);
        }
    }

    public void ToggleTheme()
    {
        SetBaseTheme(!IsDarkTheme);
        Properties.Settings.Default.Theme = IsDarkTheme ? "Dark" : "Light";
        Properties.Settings.Default.Save();
    }

    private void SetBaseTheme(bool dark)
    {
        IsDarkTheme = dark;
        try
        {
            var app = Application.Current;
            if (app?.Resources != null)
            {
                if (dark)
                {
                    // Industry Standard Dark Mode Colors (VS Code, GitHub, Discord style)
                    // Background/Surface - Deep dark for reduced eye strain
                    app.Resources["MaterialDesignBackground"] = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x12)); // #121212 - Pure dark background
                    app.Resources["MaterialDesignPaper"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E)); // #1E1E1E - Card/surface (VS Code style)
                    
                    // Text - High contrast for readability
                    app.Resources["MaterialDesignBody"] = new SolidColorBrush(Color.FromRgb(0xE4, 0xE4, 0xE7)); // #E4E4E7 - Primary text (high contrast white)
                    app.Resources["MaterialDesignSecondaryTextBrush"] = new SolidColorBrush(Color.FromRgb(0xA1, 0xA1, 0xAA)); // #A1A1AA - Secondary text (visible gray)
                    
                    // Dividers/Outlines - Visible but subtle
                    app.Resources["MaterialDesignDivider"] = new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x46)); // #3F3F46 - Dividers/borders (visible on dark)
                    
                    // Primary colors - Bright and visible
                    app.Resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0x63, 0x66, 0xF1)); // #6366F1 - Indigo primary (bright, visible)
                    app.Resources["OnPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)); // #FFFFFF - White text on primary
                    app.Resources["PrimaryContainerBrush"] = new SolidColorBrush(Color.FromRgb(0x4F, 0x46, 0xE5)); // #4F46E5 - Darker indigo for containers
                    app.Resources["OnPrimaryContainerBrush"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE7, 0xFF)); // #E0E7FF - Light indigo text
                    
                    // Error colors - Bright and attention-grabbing
                    app.Resources["ErrorBrush"] = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // #EF4444 - Bright red (highly visible)
                    app.Resources["OnErrorBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)); // #FFFFFF - White text on error
                    
                    // Success colors - Bright green
                    app.Resources["SuccessBrush"] = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)); // #22C55E - Bright green
                    
                    // Warning colors - Bright amber
                    app.Resources["WarningBrush"] = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)); // #F59E0B - Bright amber
                    
                    // Surface variants for containers - Subtle elevation
                    app.Resources["SurfaceContainerHighBrush"] = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x25)); // #252525 - Elevated surfaces (dialogs)
                    app.Resources["SurfaceContainerLowBrush"] = new SolidColorBrush(Color.FromRgb(0x18, 0x18, 0x18)); // #181818 - Slightly elevated
                    app.Resources["SurfaceVariantBrush"] = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A)); // #2A2A2A - Variant surfaces
                }
                else
                {
                    // Light theme (keeping existing values)
                    app.Resources["MaterialDesignPaper"] = new SolidColorBrush(Colors.White);
                    app.Resources["MaterialDesignBody"] = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21));
                    app.Resources["MaterialDesignDivider"] = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
                    app.Resources["MaterialDesignBackground"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
                    app.Resources["MaterialDesignSecondaryTextBrush"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
                }
            }
        }
        catch
        {
            // If theme switching fails, just update the flag
            // The app will still work with the default dark theme
        }
    }
}


