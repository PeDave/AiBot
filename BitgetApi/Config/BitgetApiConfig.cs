namespace BitgetApi.Config;

public class BitgetApiConfig
{
    public string ApiKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string Passphrase { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.bitget.com";
    public bool EnableDebugLogging { get; set; } = false;
}
