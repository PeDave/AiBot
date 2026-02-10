using System.Security.Cryptography;
using System.Text;
using BitgetApi.Models;

namespace BitgetApi.Auth;

/// <summary>
/// Handles authentication for Bitget API requests
/// </summary>
public class BitgetAuthenticator
{
    private readonly BitgetCredentials _credentials;

    public BitgetAuthenticator(BitgetCredentials credentials)
    {
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        
        if (string.IsNullOrWhiteSpace(_credentials.ApiKey))
            throw new ArgumentException("API Key cannot be empty", nameof(credentials));
        
        if (string.IsNullOrWhiteSpace(_credentials.SecretKey))
            throw new ArgumentException("Secret Key cannot be empty", nameof(credentials));
        
        if (string.IsNullOrWhiteSpace(_credentials.Passphrase))
            throw new ArgumentException("Passphrase cannot be empty", nameof(credentials));
    }

    /// <summary>
    /// Gets the current timestamp in milliseconds
    /// </summary>
    public string GetTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
    }

    /// <summary>
    /// Generates HMAC-SHA256 signature for API request
    /// </summary>
    /// <param name="timestamp">Request timestamp</param>
    /// <param name="method">HTTP method (GET, POST, DELETE)</param>
    /// <param name="requestPath">API endpoint path</param>
    /// <param name="body">Request body (empty string for GET/DELETE)</param>
    /// <returns>Base64 encoded signature</returns>
    public string GenerateSignature(string timestamp, string method, string requestPath, string body = "")
    {
        // Bitget signature format: timestamp + method + requestPath + body
        var message = timestamp + method.ToUpper() + requestPath + body;
        
        return ComputeHmacSha256(message, _credentials.SecretKey);
    }

    /// <summary>
    /// Adds authentication headers to the request
    /// </summary>
    /// <param name="headers">Headers dictionary to modify</param>
    /// <param name="method">HTTP method</param>
    /// <param name="requestPath">API endpoint path</param>
    /// <param name="body">Request body</param>
    public void AddAuthHeaders(Dictionary<string, string> headers, string method, string requestPath, string body = "")
    {
        var timestamp = GetTimestamp();
        var signature = GenerateSignature(timestamp, method, requestPath, body);

        headers["ACCESS-KEY"] = _credentials.ApiKey;
        headers["ACCESS-SIGN"] = signature;
        headers["ACCESS-TIMESTAMP"] = timestamp;
        headers["ACCESS-PASSPHRASE"] = _credentials.Passphrase;
        headers["Content-Type"] = "application/json";
        headers["locale"] = "en-US";
    }

    /// <summary>
    /// Computes HMAC-SHA256 hash
    /// </summary>
    private string ComputeHmacSha256(string message, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        
        return Convert.ToBase64String(hashBytes);
    }

    public BitgetCredentials Credentials => _credentials;
}
