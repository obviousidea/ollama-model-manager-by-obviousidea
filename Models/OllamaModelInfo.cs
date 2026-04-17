namespace OllamaModelManagerByObviousIdea.Models;

public sealed class OllamaModelInfo
{
    public required string Name { get; init; }

    public required string Size { get; init; }

    public required string Modified { get; init; }

    public required double SizeSortValue { get; init; }

    public required DateTime ModifiedSortValue { get; init; }

    public bool IsVision { get; init; }
}
