using System.Windows;
using System.Windows.Media;

namespace BarcodeApp.Services;

public interface IThemeService
{
    void ApplySavedTheme();
}

public class ThemeService : IThemeService
{
    public void ApplySavedTheme()
    {
        // Always apply dark theme
        SetDarkTheme();
    }

    private void SetDarkTheme()
    {
        try
        {
            var app = Application.Current;
            if (app?.Resources != null)
            {
                    // Custom Dark Theme Colors (from CSS variables)
                    // Base
                    app.Resources["MaterialDesignBackground"] = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)); // #0F172A - window background
                    app.Resources["MaterialDesignPaper"] = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)); // #111827 - cards, panels
                    app.Resources["BgHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)); // #1E293B - hover background
                    app.Resources["MaterialDesignDivider"] = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x37)); // #1F2937 - borders
                    
                    // Text - All white/very light for visibility
                    app.Resources["MaterialDesignBody"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)); // #FFFFFF - main text (white)
                    app.Resources["MaterialDesignSecondaryTextBrush"] = new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)); // #F3F4F6 - secondary text (very light gray, almost white)
                    app.Resources["TextSubtleBrush"] = new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB)); // #D1D5DB - hints, help text (light gray, visible)
                    app.Resources["TextInvertedBrush"] = new SolidColorBrush(Color.FromRgb(0x02, 0x06, 0x17)); // #020617 - text on bright buttons
                    
                    // Primary brand / actions (blue)
                    app.Resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)); // #3B82F6 - primary
                    app.Resources["PrimaryHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)); // #2563EB - primary hover
                    app.Resources["PrimaryActiveBrush"] = new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8)); // #1D4ED8 - primary active
                    app.Resources["OnPrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)); // White text on primary
                    app.Resources["PrimaryContainerBrush"] = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)); // Use hover for container
                    app.Resources["OnPrimaryContainerBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)); // White text
                    
                    // Secondary accent (teal) - Brighter for visibility
                    app.Resources["SecondaryBrush"] = new SolidColorBrush(Color.FromRgb(0x2D, 0xDD, 0xD4)); // #2DDDD4 - brighter teal
                    app.Resources["SecondaryHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x14, 0xB8, 0xA6)); // #14B8A6 - secondary hover
                    app.Resources["AccentBrush"] = new SolidColorBrush(Color.FromRgb(0x2D, 0xDD, 0xD4)); // Brighter accent
                    
                    // Inputs / fields - Lighter than background to show it's inputable
                    app.Resources["InputBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)); // #1E293B - lighter than background (#0F172A)
                    app.Resources["InputBorderBrush"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)); // #1E293B - input border
                    app.Resources["InputFocusBrush"] = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)); // #3B82F6 - input focus
                    app.Resources["PlaceholderBrush"] = new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF)); // #9CA3AF - placeholder (visible gray)
                    
                    // States
                    app.Resources["SuccessBrush"] = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)); // #22C55E - success
                    app.Resources["WarningBrush"] = new SolidColorBrush(Color.FromRgb(0xFA, 0xCC, 0x15)); // #FACC15 - warning
                    app.Resources["ErrorBrush"] = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // #EF4444 - error
                    app.Resources["InfoBrush"] = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8)); // #38BDF8 - info
                    app.Resources["OnErrorBrush"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)); // White text on error
                    
                    // Links
                    app.Resources["LinkBrush"] = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)); // #60A5FA - link
                    app.Resources["LinkHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x93, 0xC5, 0xFD)); // #93C5FD - link hover
                    
                    // Surface variants for containers
                app.Resources["SurfaceContainerHighBrush"] = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)); // #111827 - elevated surfaces
                app.Resources["SurfaceContainerLowBrush"] = new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)); // #0F172A - slightly elevated
                app.Resources["SurfaceVariantBrush"] = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)); // #1E293B - variant surfaces
            }
        }
        catch
        {
            // If theme application fails, the app will still work with default dark theme
        }
    }
}


