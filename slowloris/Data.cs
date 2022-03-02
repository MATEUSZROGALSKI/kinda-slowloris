using System.Collections.Concurrent;

internal record TargetInfo(string host, int port, int timeout, int concurrency);
internal record Configuration(int threadCount, int ioThreadCount, IEnumerable<TargetInfo> targets);

record HostStatistics(long bytesSent, bool isConnected, DateTime connectedEstablished);

static class Statistics
{
    private static readonly ConcurrentDictionary<string, HostStatistics> _perHostStats;

    static Statistics()
    {
        _perHostStats = new ConcurrentDictionary<string, HostStatistics>();
    }

    public static void RecordStats(string host, long newBytesCount)
    {
        if (!_perHostStats.ContainsKey(host))
        {
            _perHostStats.TryAdd(host, new(newBytesCount, false, DateTime.Now));
        }
        var current = _perHostStats[host];
        _perHostStats[host] = current with { bytesSent = current.bytesSent + newBytesCount };
    }

    public static void RecordConnectionEstablished(string host)
    {
        if (!_perHostStats.ContainsKey(host))
        {
            _perHostStats.TryAdd(host, new(0, false, DateTime.Now));
        }
        var current = _perHostStats[host];
        _perHostStats[host] = current with { isConnected = true, connectedEstablished = DateTime.Now };
    }

    public static void RecordDisconnected(string host)
    {
        if (!_perHostStats.ContainsKey(host))
        {
            _perHostStats.TryAdd(host, new(0, false, DateTime.Now));
        }
        var current = _perHostStats[host];
        _perHostStats[host] = current with { isConnected = false };
    }

    public static HostStatistics Get(string host)
    {
        return _perHostStats.ContainsKey(host) ? _perHostStats[host] : new HostStatistics(0, false, DateTime.MinValue);
    }
}
