internal record TargetInfo(string host, int port, bool isSecure, int timeout, int concurrency);
internal record Configuration(IEnumerable<TargetInfo> targets);
internal record HostStatistics(long bytesSent, bool isConnected, DateTime connectedEstablished);
internal record RuntimeConfiguration(bool showStatistics);