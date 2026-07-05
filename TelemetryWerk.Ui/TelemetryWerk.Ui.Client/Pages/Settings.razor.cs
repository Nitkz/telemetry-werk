using Microsoft.AspNetCore.Components;
using TelemetryWerk.Ui.Core.Entities;
using TelemetryWerk.Ui.Core.Interfaces;
using MudBlazor;

namespace TelemetryWerk.Ui.Client.Pages;

public partial class Settings
{
    [Inject]
    public IMachineApiService MachineApiService { get; set; } = default!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    public IDialogService DialogService { get; set; } = default!;

    [Inject]
    public NavigationManager NavManager { get; set; } = default!;

    private bool _loading = true;
    private string? _error;
    private List<UiMachineNode> _nodes = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadNodesAsync();
    }

    private async Task LoadNodesAsync()
    {
        _loading = true;
        _error = null;
        try
        {
            var result = await MachineApiService.GetNodesAsync(100);
            _nodes = result.ToList();
        }
        catch (TelemetryWerk.Api.Client.ApiException ex) when (ex.StatusCode == 401)
        {
            NavManager.NavigateTo("/login?error=Session expired");
        }
        catch (Exception ex)
        {
            _error = $"Failed to fetch data from API: {ex.Message}";
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OpenAddDialogAsync()
    {
        var parameters = new DialogParameters<NodeDialog>
        {
            { x => x.IsNewNode, true },
            { x => x.Node, new UiMachineNode() },
            { x => x.SaveAction, AddNodeAction }
        };

        var dialog = await DialogService.ShowAsync<NodeDialog>("Add New Machine Node", parameters);
        var result = await dialog.Result;

        if (!result!.Canceled && result.Data is UiMachineNode addedNode)
        {
            _nodes.Add(addedNode);
        }
    }

    private async Task<bool> AddNodeAction(UiMachineNode node)
    {
        try
        {
            var addedNode = await MachineApiService.AddNodeAsync(node);
            if (addedNode != null)
            {
                node.Id = addedNode.Id;
                node.CoreTemperature = addedNode.CoreTemperature;
                node.Status = addedNode.Status;
                Snackbar.Add("Node added successfully.", Severity.Success);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            HandleApiError(ex, "Failed to add node");
            return false;
        }
    }

    private async Task OpenEditDialogAsync(UiMachineNode existingNode)
    {
        var clonedNode = new UiMachineNode
        {
            Id = existingNode.Id,
            CoreTemperature = existingNode.CoreTemperature,
            Status = existingNode.Status
        };

        var parameters = new DialogParameters<NodeDialog>
        {
            { x => x.IsNewNode, false },
            { x => x.Node, clonedNode },
            { x => x.SaveAction, EditNodeAction }
        };

        var dialog = await DialogService.ShowAsync<NodeDialog>($"Edit Node {existingNode.Id}", parameters);
        var result = await dialog.Result;

        if (!result!.Canceled && result.Data is UiMachineNode updatedNode)
        {
            var index = _nodes.FindIndex(n => n.Id == updatedNode.Id);
            if (index != -1)
            {
                _nodes[index] = updatedNode;
            }
        }
    }

    private async Task<bool> EditNodeAction(UiMachineNode node)
    {
        try
        {
            var updatedNode = await MachineApiService.UpdateNodeAsync(node.Id, node);
            if (updatedNode != null)
            {
                Snackbar.Add($"Node {node.Id} updated successfully.", Severity.Success);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            HandleApiError(ex, "Failed to update node");
            return false;
        }
    }

    private async Task DeleteNodeAsync(string id)
    {
        var confirm = await DialogService.ShowMessageBox(
            "Confirm Delete",
            $"Are you sure you want to delete node {id}?",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirm != true)
        {
            return;
        }

        try
        {
            await MachineApiService.DeleteNodeAsync(id);
            var nodeToRemove = _nodes.FirstOrDefault(n => n.Id == id);
            if (nodeToRemove != null)
            {
                _nodes.Remove(nodeToRemove);
                Snackbar.Add($"Node {id} deleted successfully.", Severity.Success);
            }
        }
        catch (TelemetryWerk.Api.Client.ApiException ex) when (ex.StatusCode == 409)
        {
            Snackbar.Add($"Cannot delete node {id}: It is a protected system node.", Severity.Error);
        }
        catch (Exception ex)
        {
            HandleApiError(ex, $"Failed to delete node {id}");
        }
    }

    private void HandleApiError(Exception ex, string fallbackMessage)
    {
        if (ex is TelemetryWerk.Api.Client.ApiException apiEx && !string.IsNullOrEmpty(apiEx.Response))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(apiEx.Response);
                if (doc.RootElement.TryGetProperty("errors", out var errorsObj) && errorsObj.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    bool hasErrors = false;
                    foreach (var prop in errorsObj.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var msg in prop.Value.EnumerateArray())
                            {
                                Snackbar.Add($"{prop.Name}: {msg.GetString()}", Severity.Error);
                                hasErrors = true;
                            }
                        }
                    }
                    if (hasErrors) return;
                }
                else if (doc.RootElement.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    Snackbar.Add($"{fallbackMessage}: {msgProp.GetString()}", Severity.Error);
                    return;
                }
            }
            catch { /* Parse failed, fallback below */ }
        }

        Snackbar.Add($"{fallbackMessage}: {ex.Message}", Severity.Error);
    }
}
