using BitgetApi.Auth;
using BitgetApi.Http;
using BitgetApi.Models;
using BitgetApi.RestApi.Broker;
using BitgetApi.RestApi.Common;
using BitgetApi.RestApi.Convert;
using BitgetApi.RestApi.CopyTrading;
using BitgetApi.RestApi.Earn;
using BitgetApi.RestApi.Futures;
using BitgetApi.RestApi.Margin;
using BitgetApi.RestApi.Spot;
using BitgetApi.RestApi.Tax;
using BitgetApi.WebSocket;
using BitgetApi.WebSocket.Private;
using BitgetApi.WebSocket.Public;
using Microsoft.Extensions.Logging;

namespace BitgetApi;

/// <summary>
/// Main client for accessing Bitget Exchange API
/// </summary>
public class BitgetApiClient : IDisposable
{
    private readonly BitgetHttpClient _httpClient;
    private readonly BitgetWebSocketClient _webSocketClient;
    private readonly BitgetAuthenticator? _authenticator;

    // REST API Clients
    public CommonApiClient Common { get; }
    public SpotMarketClient SpotMarket { get; }
    public SpotAccountClient SpotAccount { get; }
    public SpotTradeClient SpotTrade { get; }
    public FuturesMarketClient FuturesMarket { get; }
    public FuturesAccountClient FuturesAccount { get; }
    public FuturesTradeClient FuturesTrade { get; }
    public MarginAccountClient MarginAccount { get; }
    public EarnClient Earn { get; }
    public CopyTradingClient CopyTrading { get; }
    public BrokerClient Broker { get; }
    public ConvertClient Convert { get; }
    public TaxClient Tax { get; }

    // WebSocket
    public BitgetWebSocketClient WebSocket => _webSocketClient;
    
    // WebSocket Channels
    public SpotPublicChannels SpotPublicChannels { get; }
    public FuturesPublicChannels FuturesPublicChannels { get; }
    public SpotPrivateChannels SpotPrivateChannels { get; }
    public FuturesPrivateChannels FuturesPrivateChannels { get; }

    /// <summary>
    /// Create a new Bitget API client
    /// </summary>
    /// <param name="credentials">API credentials (optional for public endpoints only)</param>
    /// <param name="logger">Logger instance (optional)</param>
    public BitgetApiClient(BitgetCredentials? credentials = null, ILogger<BitgetApiClient>? logger = null)
    {
        _authenticator = credentials != null ? new BitgetAuthenticator(credentials) : null;
        _httpClient = new BitgetHttpClient(_authenticator);
        _webSocketClient = new BitgetWebSocketClient(_authenticator);

        // Initialize REST API clients
        Common = new CommonApiClient(_httpClient);
        SpotMarket = new SpotMarketClient(_httpClient);
        SpotAccount = new SpotAccountClient(_httpClient);
        SpotTrade = new SpotTradeClient(_httpClient);
        FuturesMarket = new FuturesMarketClient(_httpClient);
        FuturesAccount = new FuturesAccountClient(_httpClient);
        FuturesTrade = new FuturesTradeClient(_httpClient);
        MarginAccount = new MarginAccountClient(_httpClient);
        Earn = new EarnClient(_httpClient);
        CopyTrading = new CopyTradingClient(_httpClient);
        Broker = new BrokerClient(_httpClient);
        Convert = new ConvertClient(_httpClient);
        Tax = new TaxClient(_httpClient);

        // Initialize WebSocket channels
        SpotPublicChannels = new SpotPublicChannels(_webSocketClient);
        FuturesPublicChannels = new FuturesPublicChannels(_webSocketClient);
        SpotPrivateChannels = new SpotPrivateChannels(_webSocketClient);
        FuturesPrivateChannels = new FuturesPrivateChannels(_webSocketClient);
    }

    /// <summary>
    /// Create a client with credentials from configuration
    /// </summary>
    public static BitgetApiClient Create(string apiKey, string secretKey, string passphrase, ILogger<BitgetApiClient>? logger = null)
    {
        var credentials = new BitgetCredentials
        {
            ApiKey = apiKey,
            SecretKey = secretKey,
            Passphrase = passphrase
        };

        return new BitgetApiClient(credentials, logger);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _webSocketClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
