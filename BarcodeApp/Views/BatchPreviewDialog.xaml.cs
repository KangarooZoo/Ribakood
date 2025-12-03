using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BarcodeApp.ViewModels;

namespace BarcodeApp.Views;

public partial class BatchPreviewDialog : Window
{
    public BatchPreviewDialog(BatchPreviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Apply light theme to this window's resources
        ApplyLightTheme();
        
        viewModel.CloseRequested += (sender, e) =>
        {
            DialogResult = true;
            Close();
        };
    }
    
    private void ApplyLightTheme()
    {
        if (Resources != null)
        {
            Resources["MaterialDesignPaper"] = new SolidColorBrush(Colors.White);
            Resources["MaterialDesignBody"] = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21));
            Resources["MaterialDesignDivider"] = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
            Resources["MaterialDesignBackground"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
            Resources["MaterialDesignSecondaryTextBrush"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
        }
    }
    
    private void PreviewItem_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is BatchPreviewItem item)
        {
            if (DataContext is BatchPreviewViewModel viewModel)
            {
                viewModel.SelectedItem = item;
            }
        }
    }
}

