using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WebView2Test.Theme;

public enum AppTheme { Light, Dark }
public enum ThemeChoice { System, Light, Dark }

public static class ThemeManager
{
    public static bool IsSystemDarkMode()
    {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var key = Registry.CurrentUser.OpenSubKey(keyPath);
        return key?.GetValue("AppsUseLightTheme") is int i && i == 0;
    }

    public static void Apply(AppTheme theme)
    {
        var dicts = Application.Current.Resources.MergedDictionaries;

        var old = dicts.FirstOrDefault(d =>
            d.Source?.OriginalString.Contains("Light") == true ||
            d.Source?.OriginalString.Contains("Dark") == true);
        if (old != null)
            dicts.Remove(old);

        var uri = theme == AppTheme.Dark
            ? "pack://application:,,,/Theme/Dark.xaml"
            : "pack://application:,,,/Theme/Light.xaml";

        dicts.Add(new ResourceDictionary { Source = new Uri(uri) });
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    public static void SetTitleBar(Window window, AppTheme theme)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
            return;
        int dark = theme == AppTheme.Dark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, 20 /* DWMWA_USE_IMMERSIVE_DARK_MODE */, ref dark, sizeof(int));
    }
}
