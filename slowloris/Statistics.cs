using System.Collections.Concurrent;

internal class StatisticsUpdateEventArgs : EventArgs 
{
    public readonly string Hostname;
    public readonly HostStatistics Statistics;

    public StatisticsUpdateEventArgs(string host, HostStatistics stats)
    {
        Hostname = host;
        Statistics = stats;
    }
}

internal class Statistics
{
    private static Statistics? _instance;
    public static Statistics Instance => _instance ??= new Statistics();
    
    private readonly ConcurrentDictionary<string, HostStatistics> _statistics;

    public EventHandler<StatisticsUpdateEventArgs>? OnStatisticsUpdated;

    private Statistics()
    {
        _statistics = new ConcurrentDictionary<string, HostStatistics>();
    }

    public void RecordBytesSent(string host, long newBytesCount)
    {
        var result = _statistics.AddOrUpdate(
            host,
            new HostStatistics(newBytesCount, true, DateTime.Now),
            (h, p) =>
            {
                return p with { bytesSent = p.bytesSent + newBytesCount };
            });
        OnStatisticsUpdated?.Invoke(this, new(host, result));
    }

    public void RecordConnected(string host)
    {
        var result = _statistics.AddOrUpdate(
            host,
            new HostStatistics(0, true, DateTime.Now),
            (h, p) =>
            {
                return p with { isConnected = true };
            });
        OnStatisticsUpdated?.Invoke(this, new(host, result));
    }

    public void RecordDisconnected(string host)
    {
        var result = _statistics.AddOrUpdate(
            host,
            new HostStatistics(0, false, DateTime.Now),
            (h, p) =>
            {
                return p with { isConnected = false };
            });
        OnStatisticsUpdated?.Invoke(this, new(host, result));
    }

    public HostStatistics GetStatisticsOf(string host)
    {
        return _statistics.GetOrAdd(
            host,
            new HostStatistics(0, false, DateTime.Now));
    }

    public IEnumerable<string> GetHosts()
    {
        return _statistics.Keys;
    }
}