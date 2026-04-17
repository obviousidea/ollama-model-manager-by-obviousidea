using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using OllamaModelManagerByObviousIdea.Infrastructure;
using OllamaModelManagerByObviousIdea.Models;
using OllamaModelManagerByObviousIdea.Services;

namespace OllamaModelManagerByObviousIdea.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly OllamaCliService _ollamaCliService = new();
    private readonly ObservableCollection<OllamaModelInfo> _allModels = [];

    private OllamaModelInfo? _selectedModel;
    private string _searchText = string.Empty;
    private string _statusText = "Ready.";
    private string _summaryText = "Loading models...";
    private string _emptyStateText = "Refresh to query Ollama.";
    private Visibility _emptyStateVisibility = Visibility.Collapsed;
    private string _spaceSummaryText = "0 B shown";

    public MainViewModel()
    {
        FilteredModels = [];
        RefreshCommand = new AsyncRelayCommand(_ => RefreshAsync());
        DeleteModelCommand = new AsyncRelayCommand(DeleteModelAsync, parameter => parameter is OllamaModelInfo);
        RepullModelCommand = new AsyncRelayCommand(RepullModelAsync, parameter => parameter is OllamaModelInfo);
        OpenOllamaFolderCommand = new AsyncRelayCommand(_ => OpenOllamaFolderAsync());
        AppVersion = $"Version {Assembly.GetExecutingAssembly().GetName().Version}";
    }

    public ObservableCollection<OllamaModelInfo> FilteredModels { get; }

    public ICommand RefreshCommand { get; }

    public ICommand DeleteModelCommand { get; }

    public ICommand RepullModelCommand { get; }

    public ICommand OpenOllamaFolderCommand { get; }

    public string AppVersion { get; }

    public OllamaModelInfo? SelectedModel
    {
        get => _selectedModel;
        set => SetProperty(ref _selectedModel, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
                OnPropertyChanged(nameof(SearchPlaceholderVisibility));
            }
        }
    }

    public Visibility SearchPlaceholderVisibility =>
        string.IsNullOrWhiteSpace(SearchText) ? Visibility.Visible : Visibility.Collapsed;

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string SummaryText
    {
        get => _summaryText;
        private set => SetProperty(ref _summaryText, value);
    }

    public string EmptyStateText
    {
        get => _emptyStateText;
        private set => SetProperty(ref _emptyStateText, value);
    }

    public string SpaceSummaryText
    {
        get => _spaceSummaryText;
        private set => SetProperty(ref _spaceSummaryText, value);
    }

    public Visibility EmptyStateVisibility
    {
        get => _emptyStateVisibility;
        private set => SetProperty(ref _emptyStateVisibility, value);
    }

    public Task InitializeAsync()
    {
        return RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            StatusText = $"Querying {_ollamaCliService.OllamaExecutablePath}...";
            var models = await _ollamaCliService.ListModelsAsync();

            _allModels.Clear();
            foreach (var model in models.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
            {
                _allModels.Add(model);
            }

            ApplyFilter();
            SummaryText = $"{_allModels.Count} installed model(s)";
            SpaceSummaryText = $"{FormatBytes(_allModels.Sum(m => m.SizeSortValue))} shown";
            StatusText = "Ollama library synchronized.";
            EmptyStateText = "No models match the current filter.";
        }
        catch (Exception ex)
        {
            _allModels.Clear();
            ApplyFilter();
            SummaryText = "Unable to load models";
            SpaceSummaryText = "0 B shown";
            StatusText = ex.Message.Trim();
            EmptyStateText = "Make sure the Ollama service is running and ollama.exe is reachable.";
        }
    }

    private async Task DeleteModelAsync(object? parameter)
    {
        if (parameter is not OllamaModelInfo model)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Delete '{model.Name}' from the local Ollama library?",
            "Confirm deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            StatusText = $"Deleting {model.Name}...";
            await _ollamaCliService.DeleteModelAsync(model.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusText = ex.Message.Trim();
            MessageBox.Show(
                $"Deletion failed.\n\n{ex.Message}",
                "Delete failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task RepullModelAsync(object? parameter)
    {
        if (parameter is not OllamaModelInfo model)
        {
            return;
        }

        var result = MessageBox.Show(
            $"Re-pull '{model.Name}' from Ollama and refresh the local copy?",
            "Confirm re-pull",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            StatusText = $"Re-pulling {model.Name}...";
            await _ollamaCliService.RepullModelAsync(model.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusText = ex.Message.Trim();
            MessageBox.Show(
                $"Re-pull failed.\n\n{ex.Message}",
                "Re-pull failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private Task OpenOllamaFolderAsync()
    {
        var path = Path.GetDirectoryName(_ollamaCliService.OllamaExecutablePath);
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            MessageBox.Show("The Ollama installation folder could not be found.", "Path not found", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });

        return Task.CompletedTask;
    }

    private void ApplyFilter()
    {
        var filtered = _allModels
            .Where(model => string.IsNullOrWhiteSpace(SearchText) ||
                            model.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        FilteredModels.Clear();
        foreach (var model in filtered)
        {
            FilteredModels.Add(model);
        }

        SpaceSummaryText = $"{FormatBytes(filtered.Sum(m => m.SizeSortValue))} shown";
        EmptyStateVisibility = FilteredModels.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string FormatBytes(double bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = bytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.#} {units[unitIndex]}";
    }
}
