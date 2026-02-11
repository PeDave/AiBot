using BitgetApi.Auth;
using BitgetApi.Models;
using Xunit;

namespace BitgetApi.Tests;

public class BitgetAuthenticatorTests
{
    [Fact]
    public void Constructor_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };

        // Act
        var authenticator = new BitgetAuthenticator(credentials);

        // Assert
        Assert.NotNull(authenticator);
        Assert.Equal(credentials, authenticator.Credentials);
    }

    [Fact]
    public void Constructor_WithNullCredentials_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BitgetAuthenticator(null!));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new BitgetAuthenticator(credentials));
    }

    [Fact]
    public void Constructor_WithEmptySecretKey_ShouldThrowArgumentException()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "",
            Passphrase = "test-passphrase"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new BitgetAuthenticator(credentials));
    }

    [Fact]
    public void Constructor_WithEmptyPassphrase_ShouldThrowArgumentException()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = ""
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new BitgetAuthenticator(credentials));
    }

    [Fact]
    public void GetTimestamp_ShouldReturnValidTimestamp()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };
        var authenticator = new BitgetAuthenticator(credentials);

        // Act
        var timestamp = authenticator.GetTimestamp();

        // Assert
        Assert.NotNull(timestamp);
        Assert.True(long.TryParse(timestamp, out var timestampValue));
        Assert.True(timestampValue > 0);
    }

    [Fact]
    public void GenerateSignature_WithValidParameters_ShouldReturnBase64String()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };
        var authenticator = new BitgetAuthenticator(credentials);
        var timestamp = "1234567890";
        var method = "GET";
        var requestPath = "/api/v2/spot/market/tickers";
        var body = "";

        // Act
        var signature = authenticator.GenerateSignature(timestamp, method, requestPath, body);

        // Assert
        Assert.NotNull(signature);
        Assert.NotEmpty(signature);
        // Verify it's valid Base64
        Assert.True(IsBase64String(signature));
    }

    [Fact]
    public void GenerateSignature_SameInputs_ShouldReturnSameSignature()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };
        var authenticator = new BitgetAuthenticator(credentials);
        var timestamp = "1234567890";
        var method = "POST";
        var requestPath = "/api/v2/spot/trade/place-order";
        var body = "{\"symbol\":\"BTCUSDT\"}";

        // Act
        var signature1 = authenticator.GenerateSignature(timestamp, method, requestPath, body);
        var signature2 = authenticator.GenerateSignature(timestamp, method, requestPath, body);

        // Assert
        Assert.Equal(signature1, signature2);
    }

    [Fact]
    public void AddAuthHeaders_ShouldAddAllRequiredHeaders()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };
        var authenticator = new BitgetAuthenticator(credentials);
        var headers = new Dictionary<string, string>();
        var method = "GET";
        var requestPath = "/api/v2/spot/account/assets";

        // Act
        authenticator.AddAuthHeaders(headers, method, requestPath);

        // Assert
        Assert.Contains("ACCESS-KEY", headers.Keys);
        Assert.Contains("ACCESS-SIGN", headers.Keys);
        Assert.Contains("ACCESS-TIMESTAMP", headers.Keys);
        Assert.Contains("ACCESS-PASSPHRASE", headers.Keys);
        Assert.Contains("Content-Type", headers.Keys);
        Assert.Contains("locale", headers.Keys);
        
        Assert.Equal("test-api-key", headers["ACCESS-KEY"]);
        Assert.Equal("test-passphrase", headers["ACCESS-PASSPHRASE"]);
        Assert.Equal("application/json", headers["Content-Type"]);
        Assert.Equal("en-US", headers["locale"]);
    }

    private static bool IsBase64String(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return false;

        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
