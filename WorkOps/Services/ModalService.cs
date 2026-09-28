using WorkOps.Models.Results;

namespace WorkOps.Services;

public class ModalService
{
    public event Func<string, string, Task>? OnShow;
    public event Func<string, string, Task<bool>>? OnConfirm;
    public event Func<string, string, IReadOnlyList<string>?, Task<ModalSelectResult>>? OnSelect;

    public async Task ShowAsync(
        string message,
        string title = "")
    {
        if (OnShow != null)
        {
            await OnShow.Invoke(message, title);
        }
    }

    public async Task<bool> ConfirmAsync(
        string message,
        string title = "確認")
    {
        if (OnConfirm != null)
        {
            return await OnConfirm.Invoke(message, title);
        }
        return false;
    }

    public async Task<ModalSelectResult> SelectAsync(
        string message,
        string title = "選択",
        IReadOnlyList<string>? options = null)
    {
        if (OnSelect != null)
        {
            return await OnSelect.Invoke(message, title, options);
        }

        return new();
    }
}
