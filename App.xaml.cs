using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

namespace OllamaModelManagerByObviousIdea;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplyWindowsTheme();
    }

    private void ApplyWindowsTheme()
    {
        var useLightTheme = true;
        try
        {
            using var personalizeKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            useLightTheme = (int?)personalizeKey?.GetValue("AppsUseLightTheme") != 0;
        }
        catch
        {
            useLightTheme = true;
        }

        SetBrush("WindowBackgroundBrush", useLightTheme ? "#FFF3F4F6" : "#FF111827");
        SetBrush("SurfaceBrush", useLightTheme ? "#FFFFFFFF" : "#FF1F2937");
        SetBrush("SurfaceAltBrush", useLightTheme ? "#FFF8FAFC" : "#FF0F172A");
        SetBrush("BorderBrush", useLightTheme ? "#FFE5E7EB" : "#FF374151");
        SetBrush("TextBrush", useLightTheme ? "#FF111827" : "#FFF9FAFB");
        SetBrush("MutedTextBrush", useLightTheme ? "#FF6B7280" : "#FF9CA3AF");
        SetBrush("AccentBrush", "#FF0F766E");
        SetBrush("AccentMutedBrush", useLightTheme ? "#FFE6FFFB" : "#FF123B39");
        SetBrush("DangerBrush", "#FFB42318");
        SetBrush("DangerMutedBrush", useLightTheme ? "#FFFEF3F2" : "#FF3B1615");
        SetBrush("HeaderBrush", useLightTheme ? "#FFF9FAFB" : "#FF18212F");
    }

    private void SetBrush(string key, string colorHex)
    {
        Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
    }
}
