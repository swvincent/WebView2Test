# WebView2Test

A WPF/.NET 10 demo application that renders GitHub-style release notes inside an embedded Microsoft WebView2 browser control, with full light/dark theming across both the WPF shell and the web content.

## Features

- GitHub-style release notes page rendered in WebView2
- Light, Dark, and System-follow theme modes
- Theme applied consistently to both the WPF window (including the Win32 title bar) and the HTML content
- Responds to OS theme changes at runtime when set to System mode

## Requirements

- Windows 10 or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Microsoft Edge WebView2 Runtime (included with Windows 10/11)

## Build & Run

```powershell
dotnet run --project source/WebView2Test.csproj
```

```powershell
dotnet build source/WebView2Test.csproj
dotnet publish source/WebView2Test.csproj -c Release
```

## How It Works

The release notes HTML is embedded as a compiled resource and loaded via `NavigateToString` — no web server or external files required. Theming works in two layers:

- **WPF shell**: swaps `Light.xaml` / `Dark.xaml` resource dictionaries at runtime and uses a DWM API call to tint the native title bar.
- **WebView2 content**: injects a `light` or `dark` CSS class on the `<html>` element via `ExecuteScriptAsync`, toggling a full set of CSS custom properties defined in the HTML.
