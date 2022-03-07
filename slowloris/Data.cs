using System.Collections.Concurrent;

// configuration of a single target
internal record TargetInfo(string host, int port, bool isSecure, int timeout, int concurrency);

// wrapper for multiple targets configuration,
// this can actually be replaced with IEnumerable<TargetInfo>
// when processing json file
internal record Configuration(IEnumerable<TargetInfo> targets);

// single entry on statistics
// this represents if target is connected
// and how many bytes were sent int total
internal record HostStatistics(long bytesSent, bool isConnected, DateTime connectedEstablished);

// statistics are kept here as it's easier
// to keep stats per run than use any kind
// of database or files
internal static class Statistics
{
    private static readonly ConcurrentDictionary<string, HostStatistics> _perHostStats;

    static Statistics()
    {
        _perHostStats = new ConcurrentDictionary<string, HostStatistics>();
    }

    // adds specified amount of bytes sent to the target
    // to statistics of targeted host
    public static void RecordStats(string host, long newBytesCount)
    {
        if (!_perHostStats.ContainsKey(host))
        {
            _perHostStats.TryAdd(host, new(newBytesCount, false, DateTime.Now));
        }
        var current = _perHostStats[host];
        _perHostStats[host] = current with { bytesSent = current.bytesSent + newBytesCount };
    }

    // call this when succesfull connection
    // is established to the target
    public static void RecordConnectionEstablished(string host)
    {
        if (!_perHostStats.ContainsKey(host))
        {
            _perHostStats.TryAdd(host, new(0, false, DateTime.Now));
        }
        var current = _perHostStats[host];
        _perHostStats[host] = current with { isConnected = true, connectedEstablished = DateTime.Now };
    }

    // call this when connection to the target
    // was rejected ( disconnected from the target )
    public static void RecordDisconnected(string host)
    {
        if (!_perHostStats.ContainsKey(host))
        {
            _perHostStats.TryAdd(host, new(0, false, DateTime.Now));
        }
        var current = _perHostStats[host];
        _perHostStats[host] = current with { isConnected = false };
    }

    // gets statistics of a single host
    public static HostStatistics Get(string host)
    {
        return _perHostStats.ContainsKey(host) ? _perHostStats[host] : new HostStatistics(0, false, DateTime.MinValue);
    }
}
