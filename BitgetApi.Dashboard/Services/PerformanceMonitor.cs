using System.Diagnostics;

namespace BitgetApi.Dashboard.Services;

public class PerformanceMonitor
{
    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private long _totalMessages = 0;
    private readonly Queue<DateTime> _recentUpdates = new();
    private readonly object _lock = new();
    
    public void RecordMessage()
    {
        Interlocked.Increment(ref _totalMessages);
        
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            _recentUpdates.Enqueue(now);
            
            // Keep only last 60 seconds of updates
            while (_recentUpdates.Count > 0 && (now - _recentUpdates.Peek()).TotalSeconds > 60)
            {
                _recentUpdates.Dequeue();
            }
        }
    }
    
    public TimeSpan Uptime => _uptime.Elapsed;
    
    public long TotalMessages => _totalMessages;
    
    public double UpdatesPerSecond
    {
        get
        {
            lock (_lock)
            {
                if (_recentUpdates.Count < 2) return 0;
                
                var timespan = (DateTime.UtcNow - _recentUpdates.Peek()).TotalSeconds;
                return timespan > 0 ? _recentUpdates.Count / timespan : 0;
            }
        }
    }
    
    public string GetConnectionStatus() => "Connected";
}
