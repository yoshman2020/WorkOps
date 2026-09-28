namespace WorkOps.Models.Results;

public sealed class ModalSelectResult
{
    public bool Canceled { get; init; }
    public IReadOnlyList<string> SelectedOptions { get; init; } = [];
}
