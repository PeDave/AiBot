using BitgetApi;
using BitgetApi.Models;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace BitgetApi.IntegrationTests;

public abstract class TestBase : IDisposable
{
    protected readonly IConfiguration Configuration;
    protected readonly BitgetApiClient PublicClient;
    protected readonly BitgetApiClient? AuthenticatedClient;
    protected readonly ITestOutputHelper Output;
    protected readonly string TestSymbol;
    protected readonly string TestFuturesSymbol;
    protected readonly string TestProductType;
    protected readonly bool SkipPrivateTests;

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;

        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        TestSymbol = Configuration["TestSettings:TestSymbol"] ?? "BTCUSDT";
        TestFuturesSymbol = Configuration["TestSettings:TestFuturesSymbol"] ?? "BTCUSDT";
        TestProductType = Configuration["TestSettings:TestProductType"] ?? "USDT-FUTURES";
        SkipPrivateTests = bool.Parse(Configuration["TestSettings:SkipPrivateTests"] ?? "true");

        PublicClient = new BitgetApiClient();

        var apiKey = Configuration["Bitget:ApiKey"];
        var secretKey = Configuration["Bitget:SecretKey"];
        var passphrase = Configuration["Bitget:Passphrase"];

        if (!string.IsNullOrEmpty(apiKey) && 
            !string.IsNullOrEmpty(secretKey) && 
            !string.IsNullOrEmpty(passphrase))
        {
            var credentials = new BitgetCredentials
            {
                ApiKey = apiKey,
                SecretKey = secretKey,
                Passphrase = passphrase
            };
            AuthenticatedClient = new BitgetApiClient(credentials);
        }
    }

    public void Dispose()
    {
        PublicClient?.Dispose();
        AuthenticatedClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected void Log(string message)
    {
        Output.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    protected bool CanRunAuthenticatedTests()
    {
        return AuthenticatedClient != null && !SkipPrivateTests;
    }
}
