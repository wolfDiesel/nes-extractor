using System.Reflection;
using Avalonia.Controls;
using NesExtractor.Localization;

namespace NesExtractor.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
        if (this.FindControl<TextBlock>("VersionText") is { } versionText)
        {
            versionText.Text = string.Format(LocalizationManager.GetString("About.Version"), version);
        }
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}


