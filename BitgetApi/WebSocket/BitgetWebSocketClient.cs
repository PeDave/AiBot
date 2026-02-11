using System.Text.Json;
using System.Text.Json.Serialization;
using BitgetApi.Auth;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace BitgetApi.WebSocket;

public class WebSocketMessage
{
    [JsonPropertyName("op")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public List<WebSocketChannel>? Args { get; set; }
}

public class WebSocketChannel
{
    [JsonPropertyName("instType")]
    public string? InstType { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("instId")]
    public string? InstId { get; set; }
}

public class WebSocketResponse<T>
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("arg")]
    public WebSocketChannel? Arg { get; set; }

    [JsonPropertyName("data")]
    public List<T>? Data { get; set; }

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

/// <summary>
/// WebSocket client for Bitget real-time data streams
/// </summary>
public class BitgetWebSocketClient : IDisposable
{
    private WebsocketClient? _publicClient;
    private WebsocketClient? _privateClient;
    private readonly BitgetAuthenticator? _authenticator;
    private readonly ILogger<BitgetWebSocketClient>? _logger;
    private readonly Dictionary<string, List<Action<string>>> _subscriptions = new();
    private System.Threading.Timer? _pingTimer;
    private readonly bool _debugMode;

    public const string PublicWebSocketUrl = "wss://ws.bitget.com/v2/ws/public";
    public const string PrivateWebSocketUrl = "wss://ws.bitget.com/v2/ws/private";

    public event EventHandler<string>? OnMessage;
    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;
    public event EventHandler<Exception>? OnError;

    public BitgetWebSocketClient(BitgetAuthenticator? authenticator = null, ILogger<BitgetWebSocketClient>? logger = null, bool debugMode = false)
    {
        _authenticator = authenticator;
        _logger = logger;
        _debugMode = debugMode;
    }

    /// <summary>
    /// Connect to public WebSocket
    /// </summary>
    public async Task ConnectPublicAsync(CancellationToken cancellationToken = default)
    {
        if (_publicClient != null && _publicClient.IsRunning)
            return;

        _publicClient = new WebsocketClient(new Uri(PublicWebSocketUrl))
        {
            ReconnectTimeout = TimeSpan.FromSeconds(30)
        };

        _publicClient.ReconnectionHappened.Subscribe(info =>
        {
            _logger?.LogInformation("Public WebSocket reconnected: {Type}", info.Type);
            OnConnected?.Invoke(this, EventArgs.Empty);
        });

        _publicClient.DisconnectionHappened.Subscribe(info =>
        {
            _logger?.LogWarning("Public WebSocket disconnected: {Type}", info.Type);
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        });

        _publicClient.MessageReceived.Subscribe(msg =>
        {
            if (msg.Text != null)
                HandleMessage(msg.Text);
        });

        await _publicClient.Start();
        StartPingTimer();
    }

    /// <summary>
    /// Connect to private WebSocket (requires authentication)
    /// </summary>
    public async Task ConnectPrivateAsync(CancellationToken cancellationToken = default)
    {
        if (_authenticator == null)
            throw new InvalidOperationException("Authentication required for private WebSocket");

        if (_privateClient != null && _privateClient.IsRunning)
            return;

        _privateClient = new WebsocketClient(new Uri(PrivateWebSocketUrl))
        {
            ReconnectTimeout = TimeSpan.FromSeconds(30)
        };

        _privateClient.ReconnectionHappened.Subscribe(async info =>
        {
            _logger?.LogInformation("Private WebSocket reconnected: {Type}", info.Type);
            await AuthenticatePrivateAsync();
            OnConnected?.Invoke(this, EventArgs.Empty);
        });

        _privateClient.DisconnectionHappened.Subscribe(info =>
        {
            _logger?.LogWarning("Private WebSocket disconnected: {Type}", info.Type);
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        });

        _privateClient.MessageReceived.Subscribe(msg =>
        {
            if (msg.Text != null)
                HandleMessage(msg.Text);
        });

        await _privateClient.Start();
        await AuthenticatePrivateAsync();
        StartPingTimer();
    }

    private async Task AuthenticatePrivateAsync()
    {
        if (_authenticator == null || _privateClient == null)
            return;

        var timestamp = _authenticator.GetTimestamp();
        var signature = _authenticator.GenerateSignature(timestamp, "GET", "/user/verify", "");

        var authMessage = new
        {
            op = "login",
            args = new[]
            {
                new
                {
                    apiKey = _authenticator.Credentials.ApiKey,
                    passphrase = _authenticator.Credentials.Passphrase,
                    timestamp,
                    sign = signature
                }
            }
        };

        var json = JsonSerializer.Serialize(authMessage);
        _privateClient.Send(json);
        _logger?.LogDebug("Sent authentication message");
    }

    /// <summary>
    /// Subscribe to a channel
    /// </summary>
    public async Task SubscribeAsync(string channel, string? instId = null, string? instType = null, bool isPrivate = false, CancellationToken cancellationToken = default)
    {
        var client = isPrivate ? _privateClient : _publicClient;

        if (client == null || !client.IsRunning)
        {
            if (isPrivate)
                await ConnectPrivateAsync(cancellationToken);
            else
                await ConnectPublicAsync(cancellationToken);

            client = isPrivate ? _privateClient : _publicClient;
        }

        if (client == null)
            throw new InvalidOperationException("WebSocket client is not connected");

        var args = new WebSocketChannel
        {
            InstType = instType,
            Channel = channel,
            InstId = instId
        };

        var message = new WebSocketMessage
        {
            Operation = "subscribe",
            Args = new List<WebSocketChannel> { args }
        };

        var json = JsonSerializer.Serialize(message);

        if (_debugMode)
        {
            System.Console.WriteLine($"[WebSocket] Subscribing: {json}");
        }

        client.Send(json);
        _logger?.LogDebug("Subscribed to channel: {Channel}", channel);

        // ✅ Wait a bit to ensure subscription is processed
        await Task.Delay(100);
    }

    /// <summary>
    /// Unsubscribe from a channel
    /// </summary>
    public void Unsubscribe(string channel, string? instId = null, bool isPrivate = false)
    {
        var client = isPrivate ? _privateClient : _publicClient;
        
        if (client == null || !client.IsRunning)
            return;

        var args = new WebSocketChannel
        {
            Channel = channel,
            InstId = instId
        };

        var message = new WebSocketMessage
        {
            Operation = "unsubscribe",
            Args = new List<WebSocketChannel> { args }
        };

        var json = JsonSerializer.Serialize(message);
        client.Send(json);
        _logger?.LogDebug("Unsubscribed from channel: {Channel}", channel);
    }

    private void HandleMessage(string message)
    {
        try
        {
            _logger?.LogDebug("Received message: {Message}", message);
            OnMessage?.Invoke(this, message);

            // Check for pong response
            if (message.Contains("\"op\":\"pong\"") || message.Contains("\"event\":\"pong\""))
            {
                _logger?.LogTrace("Received pong");
                return;
            }

            // Check for subscription confirmation
            if (message.Contains("\"event\":\"subscribe\""))
            {
                _logger?.LogDebug("Subscription confirmed");
                return;
            }

            // Parse the message to get the channel and instId
            try
            {
                using var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                if (root.TryGetProperty("arg", out var arg) &&
                    arg.TryGetProperty("channel", out var channelProp))
                {
                    var channel = channelProp.GetString();
                    var instId = arg.TryGetProperty("instId", out var instIdProp)
                        ? instIdProp.GetString()
                        : null;

                    // Build the subscription key
                    if (string.IsNullOrEmpty(channel))
                    {
                        _logger?.LogTrace("Channel is null or empty");
                        return;
                    }

                    var key = instId != null ? $"{channel}_{instId}" : channel;

                    // Only call the callbacks for THIS specific channel
                    List<Action<string>> callbacks;
                    lock (_subscriptions)
                    {
                        if (_subscriptions.TryGetValue(key, out var callbackList))
                        {
                            callbacks = callbackList.ToList();
                        }
                        else
                        {
                            // No subscription found for this channel
                            _logger?.LogTrace("No subscription found for key: {Key}", key);
                            return;
                        }
                    }

                    // Call the specific callbacks
                    foreach (var callback in callbacks)
                    {
                        try
                        {
                            callback(message);
                        }
                        catch (Exception callbackEx)
                        {
                            _logger?.LogError(callbackEx, "Error in subscription callback for {Key}", key);
                        }
                    }
                }
                else
                {
                    _logger?.LogTrace("Message doesn't have 'arg' or 'channel' property");
                }
            }
            catch (JsonException)
            {
                // Not a JSON message or doesn't have the expected structure
                _logger?.LogWarning("Could not parse message structure: {Message}", message);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling WebSocket message");
            OnError?.Invoke(this, ex);
        }
    }

    private void StartPingTimer()
    {
        // Send ping every 30 seconds to keep connection alive
        _pingTimer = new System.Threading.Timer(_ =>
        {
            try
            {
                SendPing();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error sending ping");
            }
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private void SendPing()
    {
        if (_publicClient?.IsRunning == true)
        {
            _publicClient.Send("ping");
        }

        if (_privateClient?.IsRunning == true)
        {
            _privateClient.Send("ping");
        }
    }

    public void AddSubscription(string key, Action<string> callback)
    {
        lock (_subscriptions)
        {
            if (!_subscriptions.ContainsKey(key))
            {
                _subscriptions[key] = new List<Action<string>>();
            }
            _subscriptions[key].Add(callback);
        }
    }

    public void RemoveSubscription(string key)
    {
        lock (_subscriptions)
        {
            _subscriptions.Remove(key);
        }
    }

    public void Dispose()
    {
        _pingTimer?.Dispose();
        _publicClient?.Dispose();
        _privateClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
