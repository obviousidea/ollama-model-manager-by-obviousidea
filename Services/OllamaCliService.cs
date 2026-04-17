using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using OllamaModelManagerByObviousIdea.Models;

namespace OllamaModelManagerByObviousIdea.Services;

public sealed class OllamaCliService
{
    private static readonly Regex MultiSpaceRegex = new(@"\s{2,}", RegexOptions.Compiled);
    private static readonly HttpClient HttpClient = new() { BaseAddress = new Uri("http://localhost:11434/") };

    private readonly string _ollamaExePath;

    public OllamaCliService()
    {
        _ollamaExePath = ResolveOllamaPath();
    }

    public string OllamaExecutablePath => _ollamaExePath;

    public async Task<IReadOnlyList<OllamaModelInfo>> ListModelsAsync()
    {
        var output = await RunOllamaAsync("list");
        var lines = output.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).Skip(1);

        var models = new List<OllamaModelInfo>();
        foreach (var line in lines)
        {
            var columns = MultiSpaceRegex.Split(line.Trim());
            if (columns.Length < 4)
            {
                continue;
            }

            models.Add(new OllamaModelInfo
            {
                Name = columns[0],
                Size = columns[2],
                Modified = columns[3],
                SizeSortValue = ParseSize(columns[2]),
                ModifiedSortValue = ParseModified(columns[3]),
                IsVision = false
            });
        }

        return await EnrichCapabilitiesAsync(models);
    }

    public Task DeleteModelAsync(string modelName)
    {
        return RunOllamaAsync($"rm {modelName}");
    }

    public Task RepullModelAsync(string modelName)
    {
        return RunOllamaAsync($"pull {modelName}");
    }

    private async Task<string> RunOllamaAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ollamaExePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(stdErr) ? stdOut : stdErr);
        }

        return stdOut;
    }

    private static string ResolveOllamaPath()
    {
        var candidates = new[]
        {
            Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Programs\Ollama\ollama.exe"),
            Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Ollama\ollama.exe"),
            @"C:\Program Files\Ollama\ollama.exe",
            "ollama"
        };

        foreach (var candidate in candidates)
        {
            if (candidate.Equals("ollama", StringComparison.OrdinalIgnoreCase) || File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("Could not find ollama.exe. Make sure Ollama is installed.");
    }

    private static double ParseSize(string sizeText)
    {
        var parts = sizeText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return 0;
        }

        var unit = parts.Length > 1 ? parts[1].ToUpperInvariant() : "B";
        return unit switch
        {
            "KB" => value * 1024,
            "MB" => value * 1024 * 1024,
            "GB" => value * 1024 * 1024 * 1024,
            "TB" => value * 1024d * 1024d * 1024d * 1024d,
            _ => value
        };
    }

    private static DateTime ParseModified(string modifiedText)
    {
        var normalized = modifiedText.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return DateTime.MinValue;
        }

        if (normalized.StartsWith("About ", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["About ".Length..];
        }

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || !int.TryParse(parts[0], out var amount))
        {
            return DateTime.MinValue;
        }

        var unit = parts[1].ToLowerInvariant();
        var now = DateTime.Now;
        return unit switch
        {
            "minute" or "minutes" => now.AddMinutes(-amount),
            "hour" or "hours" => now.AddHours(-amount),
            "day" or "days" => now.AddDays(-amount),
            "week" or "weeks" => now.AddDays(-(amount * 7)),
            "month" or "months" => now.AddMonths(-amount),
            "year" or "years" => now.AddYears(-amount),
            _ => DateTime.MinValue
        };
    }

    private static async Task<IReadOnlyList<OllamaModelInfo>> EnrichCapabilitiesAsync(List<OllamaModelInfo> models)
    {
        var enriched = new List<OllamaModelInfo>(models.Count);

        foreach (var model in models)
        {
            var isVision = await SupportsVisionAsync(model.Name);
            enriched.Add(new OllamaModelInfo
            {
                Name = model.Name,
                Size = model.Size,
                Modified = model.Modified,
                SizeSortValue = model.SizeSortValue,
                ModifiedSortValue = model.ModifiedSortValue,
                IsVision = isVision
            });
        }

        return enriched;
    }

    private static async Task<bool> SupportsVisionAsync(string modelName)
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync("api/show", new { model = modelName });
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<ShowResponse>();
            return payload?.Capabilities?.Any(cap => string.Equals(cap, "vision", StringComparison.OrdinalIgnoreCase)) == true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class ShowResponse
    {
        public string[]? Capabilities { get; set; }
    }
}
