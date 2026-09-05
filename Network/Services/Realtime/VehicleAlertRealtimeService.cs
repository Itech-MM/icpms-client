using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using icpms_client.Network.Core;
using icpms_client.Network.DTO.AuditLogs;
using icpms_client.Utils.Settings;
using log4net;
using Microsoft.Extensions.Options;

namespace icpms_client.Network.Services.Realtime;

public class VehicleAlertRealtimeService : IAsyncDisposable
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(VehicleAlertRealtimeService));

    private readonly ApiSettings _apiSettings;
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _cts;
    private Task? _runLoop;

    public event EventHandler<VehicleAlertDto>? AlertReceived;
    
    private static readonly JsonSerializerOptions AlertJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public VehicleAlertRealtimeService(IOptions<ApiSettings> apiSettings)
    {
        _apiSettings = apiSettings.Value;
    }

    public void StartInBackground()
    {
        _cts = new CancellationTokenSource();
        _runLoop = Task.Run(() => RunAsync(_cts.Token));
    }

    private async Task RunAsync(CancellationToken token)
    {
        var delay = TimeSpan.FromSeconds(2);

        while (!token.IsCancellationRequested)
        {
            try
            {
                await ConnectAndListenAsync(token);
                delay = TimeSpan.FromSeconds(2);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Warn($"Vehicle alert WS connection lost: {ex.Message}");
            }

            if (token.IsCancellationRequested) break;

            await Task.Delay(delay, token).ContinueWith(_ => { }, TaskScheduler.Default);
            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30)); // cap backoff at 30s
        }
    }

    private async Task ConnectAndListenAsync(CancellationToken token)
    {
        _socket = new ClientWebSocket();

        var wsUri = BuildWebSocketUri(_apiSettings.BaseUrl);
        await _socket.ConnectAsync(wsUri, token);
        Log.Info($"Connected to vehicle alert WS at {wsUri}");

        // STOMP CONNECT
        await SendFrameAsync(StompFrame.Build("CONNECT", new Dictionary<string, string>
        {
            ["accept-version"] = "1.2",
            ["host"] = wsUri.Host
        }), token);

        var connectedFrame = await ReceiveFrameAsync(token);
        if (connectedFrame is null || connectedFrame.Command != "CONNECTED")
            throw new InvalidOperationException("STOMP handshake failed");

        // SUBSCRIBE
        await SendFrameAsync(StompFrame.Build("SUBSCRIBE", new Dictionary<string, string>
        {
            ["id"] = "sub-vehicle-alerts",
            ["destination"] = "/topic/vehicle-alerts"
        }), token);

        Log.Info("Subscribed to /topic/vehicle-alerts");

        while (!token.IsCancellationRequested && _socket.State == WebSocketState.Open)
        {
            var frame = await ReceiveFrameAsync(token);
            if (frame is null) break;

            if (frame.Command == "MESSAGE" && !string.IsNullOrWhiteSpace(frame.Body))
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<VehicleAlertDto>(frame.Body, AlertJsonOptions);
                    if (dto is not null)
                        AlertReceived?.Invoke(this, dto);
                }
                catch (Exception ex)
                {
                    Log.Warn($"Failed to parse vehicle alert payload: {ex.Message}");
                }
            }
            else if (frame.Command == "ERROR")
            {
                Log.Warn($"STOMP ERROR frame received: {frame.Body}");
            }
        }
    }

    private async Task SendFrameAsync(string frame, CancellationToken token)
    {
        var bytes = Encoding.UTF8.GetBytes(frame);
        await _socket!.SendAsync(bytes, WebSocketMessageType.Text, true, token);
    }

    private async Task<StompFrame?> ReceiveFrameAsync(CancellationToken token)
    {
        var buffer = new byte[8192];
        using var ms = new MemoryStream();

        while (true)
        {
            var result = await _socket!.ReceiveAsync(buffer, token);
            if (result.MessageType == WebSocketMessageType.Close)
                return null;

            ms.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                var text = Encoding.UTF8.GetString(ms.ToArray());
                if (string.IsNullOrWhiteSpace(text.Trim('\n')))
                    continue; // STOMP heartbeat (bare newline) — ignore and keep listening
                return StompFrame.Parse(text);
            }
        }
    }

    private static Uri BuildWebSocketUri(string httpBaseUrl)
    {
        var trimmed = httpBaseUrl.TrimEnd('/');
        if (trimmed.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^"/api".Length];
        }

        var uri = new Uri(trimmed + "/ws/websocket");
        var scheme = uri.Scheme == "https" ? "wss" : "ws";
        return new UriBuilder(uri) { Scheme = scheme, Port = uri.Port }.Uri;
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();

        if (_socket is { State: WebSocketState.Open })
        {
            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client shutting down",
                    CancellationToken.None);
            }
            catch { /* best-effort close */ }
        }

        if (_runLoop is not null)
        {
            try { await _runLoop; } catch { /* already logged inside loop */ }
        }

        _socket?.Dispose();
        _cts?.Dispose();
    }
}