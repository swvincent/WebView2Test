using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WebView2Test.Theme;

namespace WebView2Test;

public partial class MainWindow : Window
{
    private ThemeChoice _userChoice = ThemeChoice.System;
    private bool _webViewReady = false;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => ApplyTheme(ResolveTheme());
        Loaded += OnLoaded;
        SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
        Closed += (_, _) => SystemEvents.UserPreferenceChanged -= OnSystemThemeChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Set selection before WebView2 is ready so SelectionChanged guard works
        ThemeComboBox.SelectedIndex = 0;

        await webView.EnsureCoreWebView2Async();
        _webViewReady = true;

        webView.NavigationCompleted += async (_, _) => await ApplyThemeToWebViewAsync(ResolveTheme());

        LoadHtml();
    }

    private void LoadHtml()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream("WebView2Test.Content.ReleaseNotes.html")
            ?? throw new InvalidOperationException(
                "Embedded resource 'WebView2Test.Content.ReleaseNotes.html' not found. " +
                $"Available: {string.Join(", ", asm.GetManifestResourceNames())}");
        using var reader = new StreamReader(stream);
        webView.NavigateToString(reader.ReadToEnd());
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_webViewReady) return;

        var tag = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        _userChoice = tag switch
        {
            "Light" => ThemeChoice.Light,
            "Dark"  => ThemeChoice.Dark,
            _       => ThemeChoice.System
        };

        var theme = ResolveTheme();
        ApplyTheme(theme);
        _ = ApplyThemeToWebViewAsync(theme);
    }

    private void OnSystemThemeChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General || _userChoice != ThemeChoice.System)
            return;

        Dispatcher.Invoke(() =>
        {
            var theme = ResolveTheme();
            ApplyTheme(theme);
            _ = ApplyThemeToWebViewAsync(theme);
        });
    }

    private AppTheme ResolveTheme() => _userChoice switch
    {
        ThemeChoice.Light => AppTheme.Light,
        ThemeChoice.Dark  => AppTheme.Dark,
        _                 => ThemeManager.IsSystemDarkMode() ? AppTheme.Dark : AppTheme.Light
    };

    private void ApplyTheme(AppTheme theme)
    {
        ThemeManager.Apply(theme);
        ThemeManager.SetTitleBar(this, theme);
    }

    private async Task ApplyThemeToWebViewAsync(AppTheme theme)
    {
        if (webView.CoreWebView2 is null) return;
        string cls = theme == AppTheme.Dark ? "dark" : "light";
        await webView.CoreWebView2.ExecuteScriptAsync(
            $"document.documentElement.className = '{cls}';");
    }
}
