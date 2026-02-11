using Xunit;
using Xunit.Abstractions;

namespace BitgetApi.IntegrationTests.PublicApiTests;

public class CommonApiTests : TestBase
{
    public CommonApiTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task GetServerTime_ShouldReturnValidTimestamp()
    {
        var response = await PublicClient.Common.GetServerTimeAsync();

        Assert.NotNull(response);
        Assert.True(response.IsSuccess, $"API call failed: {response.Message}");
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data.ServerTime);
        
        var serverTime = long.Parse(response.Data.ServerTime, 
            System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var diff = Math.Abs(serverTime - now);
        
        Assert.True(diff < 60000, $"Server time differs by {diff}ms");
        Log($"✓ Server time: {serverTime} (Diff: {diff}ms)");
    }

    [Fact]
    public async Task GetAnnouncements_ShouldReturnListOrSkip()
    {
        var response = await PublicClient.Common.GetAnnouncementsAsync();
        Assert.NotNull(response);
        
        if (response.IsSuccess)
        {
            Assert.NotNull(response.Data);
            Log($"✓ Announcements: {response.Data.Count} items");
        }
        else
        {
            Log($"⚠ Announcements endpoint unavailable: {response.Message}");
        }
    }
}
