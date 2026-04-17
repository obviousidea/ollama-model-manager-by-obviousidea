using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OllamaModelManagerByObviousIdea.ViewModels;

namespace OllamaModelManagerByObviousIdea;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly Dictionary<DataGridColumn, string> _baseHeaders = [];

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        InitializeColumnHeaders();
        await _viewModel.InitializeAsync();
    }

    private void OnDataGridSorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var direction = e.Column.SortDirection != ListSortDirection.Ascending
            ? ListSortDirection.Ascending
            : ListSortDirection.Descending;

        var grid = (DataGrid)sender;
        foreach (var column in grid.Columns)
        {
            column.SortDirection = null;
            if (_baseHeaders.TryGetValue(column, out var baseHeader))
            {
                column.Header = baseHeader;
            }
        }

        var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
        view.SortDescriptions.Clear();
        view.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, direction));
        e.Column.SortDirection = direction;
        e.Column.Header = $"{_baseHeaders[e.Column]} {(direction == ListSortDirection.Ascending ? "▲" : "▼")}";
    }

    private void InitializeColumnHeaders()
    {
        foreach (var column in ModelsGrid.Columns)
        {
            _baseHeaders[column] = column.Header?.ToString() ?? string.Empty;
        }
    }
}
