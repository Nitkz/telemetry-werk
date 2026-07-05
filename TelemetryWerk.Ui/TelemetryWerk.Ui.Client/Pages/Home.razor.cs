using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System.Collections.Concurrent;
using TelemetryWerk.Ui.Core.Configurations;
using TelemetryWerk.Ui.Core.Entities;

namespace TelemetryWerk.Ui.Client.Pages;

public partial class Home : ComponentBase, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly ConcurrentDictionary<string, UiMachineTelemetry> _machines = new();
    private readonly ConcurrentQueue<double> _sparklineStream = new();
    private readonly List<UiTelemetryPackageFrame> _terminalFrames = new();
    
    private bool _isConnected;
    private long _totalFramesIngested;
    
    private PeriodicTimer? _renderTimer;
    private readonly CancellationTokenSource _cts = new();

    [Inject] public NavigationManager? NavManager { get; set; }
    [Inject] public Microsoft.Extensions.Options.IOptions<ApiServiceOptions>? ApiOptions { get; set; }

    protected override async Task OnInitializedAsync()
    {
        for (int i = 0; i < 40; i++) _sparklineStream.Enqueue(0.0);

        // Start a rendering loop to prevent UI stutter (Render 4 times a second)
        _renderTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        _ = RunRenderLoopAsync();

        if (NavManager == null) return;
        
        var endpoint = ApiOptions!.Value.ApiEndpoint;
        var apiKey = ApiOptions.Value.ApiKey;

        // HubConnectionBuilder is part of Microsoft.AspNetCore.SignalR.Client.
        // It builds the real-time two-way communication pipeline (WebSocket/SSE) between this UI and the API Server.
        _hubConnection = new HubConnectionBuilder()
            // 1. Set the target Hub URL.
            // 2. Inject the ApiKey into the AccessTokenProvider so the Server's ApiKeyMiddleware can authorize the connection.
            .WithUrl($"{endpoint}/hubs/telemetry", options => 
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(apiKey);
            })
            // Automatically attempt to reconnect (at 0, 2, 10, and 30 seconds) if the connection drops.
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<UiTelemetryPackageFrame>("ReceiveTelemetryPackage", async (packet) =>
        {
            _totalFramesIngested++;

            foreach (var metric in packet.Metrics)
            {
                _machines[metric.Id] = metric;
            }

            if (packet.Metrics.Any())
            {
                var averageTemp = packet.Metrics.Average(m => m.CoreTemperature);
                _sparklineStream.Enqueue(averageTemp);
                if (_sparklineStream.Count > 50) _sparklineStream.TryDequeue(out _);
            }

            _terminalFrames.Insert(0, packet);
            if (_terminalFrames.Count > 10) _terminalFrames.RemoveAt(_terminalFrames.Count - 1);

        });

        // Fire and forget the connection loop
        _ = ConnectWithRetryAsync();
    }

    private async Task ConnectWithRetryAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try 
            {
                await _hubConnection!.StartAsync(_cts.Token);
                _isConnected = _hubConnection.State == HubConnectionState.Connected;
                await InvokeAsync(StateHasChanged);
                break; // Exit loop if successful
            } 
            catch (Exception)
            {
                _isConnected = false;
                await InvokeAsync(StateHasChanged);
                
                // Wait 5 seconds before retrying
                try { await Task.Delay(5000, _cts.Token); } 
                catch (TaskCanceledException) { break; }
            }
        }
    }

    private async Task RunRenderLoopAsync()
    {
        try 
        {
            while (await _renderTimer!.WaitForNextTickAsync(_cts.Token))
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException) { }
    }


    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _renderTimer?.Dispose();

        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
