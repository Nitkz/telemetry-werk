using Microsoft.AspNetCore.Components;
using MudBlazor;
using TelemetryWerk.Ui.Core.Entities;

namespace TelemetryWerk.Ui.Client.Pages;

public partial class NodeDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public UiMachineNode Node { get; set; } = new();
    [Parameter] public bool IsNewNode { get; set; } = true;
    [Parameter] public Func<UiMachineNode, Task<bool>> SaveAction { get; set; } = default!;

    private MudForm _form = default!;
    private bool _isSaving;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task Submit()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _isSaving = true;
        try
        {
            var success = await SaveAction(Node);
            if (success)
            {
                MudDialog.Close(DialogResult.Ok(Node));
            }
        }
        finally
        {
            _isSaving = false;
        }
    }
}
