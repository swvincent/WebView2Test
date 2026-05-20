# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
# Build
dotnet build source/WebView2Test.csproj

# Run
dotnet run --project source/WebView2Test.csproj

# Build Release
dotnet publish source/WebView2Test.csproj -c Release
```

There are no tests in this project.

## Architecture

This is a WPF/.NET 10 desktop application (`net10.0-windows`) that displays GitHub-style release notes inside an embedded WebView2 browser control.

**Theme system — two parallel layers:**

1. **WPF layer** (`source/Theme/`): `ThemeManager.Apply()` swaps `Light.xaml` / `Dark.xaml` resource dictionaries at runtime by removing the old one and adding the new one to `Application.Current.Resources.MergedDictionaries`. `ThemeManager.SetTitleBar()` uses a DWM P/Invoke (`DWMWA_USE_IMMERSIVE_DARK_MODE`, attr 20) to color the Win32 title bar.

2. **WebView2 layer**: The HTML in `source/Content/ReleaseNotes.html` is embedded as a manifest resource and loaded via `NavigateToString`. Theme is applied to it by injecting a CSS class (`"light"` or `"dark"`) on `<html>` via `ExecuteScriptAsync`. The HTML also contains a `@media (prefers-color-scheme: dark)` fallback for the initial render before C# injects the class.

**Initialization order matters** (`MainWindow.xaml.cs`): `ThemeComboBox.SelectedIndex` is set before `EnsureCoreWebView2Async()` completes to avoid the `SelectionChanged` guard firing prematurely. The `NavigationCompleted` handler is wired after `EnsureCoreWebView2Async()` returns so theme is applied on every navigation.

**ThemeChoice vs AppTheme**: `ThemeChoice` (System/Light/Dark) is what the user picks. `AppTheme` (Light/Dark) is the resolved value. `ResolveTheme()` calls `ThemeManager.IsSystemDarkMode()` (reads `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme`) when the user choice is System.

System theme changes are handled by subscribing to `SystemEvents.UserPreferenceChanged` and filtering for `UserPreferenceCategory.General`.

## Key Files

- `source/MainWindow.xaml.cs` — all app logic (theme switching, WebView2 initialization)
- `source/Theme/ThemeManager.cs` — WPF resource dict swapping + DWM title bar tinting
- `source/Theme/Light.xaml` / `Dark.xaml` — four `SolidColorBrush` resources used by XAML bindings
- `source/Content/ReleaseNotes.html` — self-contained HTML/CSS with light+dark CSS custom properties; embedded as a resource
