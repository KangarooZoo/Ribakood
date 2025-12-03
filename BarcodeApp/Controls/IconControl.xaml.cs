using System.Windows;
using System.Windows.Controls;

namespace BarcodeApp.Controls;

public partial class IconControl : UserControl
{
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(string), typeof(IconControl),
            new PropertyMetadata(string.Empty, OnKindChanged));

    public string Kind
    {
        get => (string)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public IconControl()
    {
        InitializeComponent();
    }

    private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconControl control && e.NewValue is string kind)
        {
            control.IconText.Text = kind switch
            {
                "CheckCircle" => "✓",
                "AlertCircle" => "⚠",
                "Error" => "✕",
                "Info" => "ℹ",
                "Warning" => "⚠",
                _ => ""
            };
        }
    }
}

