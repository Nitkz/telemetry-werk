using Microsoft.AspNetCore.Components;
using TelemetryWerk.Ui.Core.Entities;
using TelemetryWerk.Ui.Core.Interfaces;
using MudBlazor;

namespace TelemetryWerk.Ui.Client.Pages;

public partial class Settings
{
    [Inject]
    public IMachineApiService MachineApiService { get; set; } = default!;

    private bool _loading = true;
    private string? _error;
    private List<UiMachineNode> _nodes = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await MachineApiService.GetNodesAsync(100);
            _nodes = result.ToList();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _error = "Unauthorized: Invalid API Key. Please check the API configuration.";
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


}
