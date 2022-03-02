internal class FlooderProcess
{
    private readonly Configuration _configuration;

    public bool IsFlooding { get => ThreadPool.PendingWorkItemCount > 0; }

    public FlooderProcess(Configuration configuration)
    {
        _configuration = configuration;
    }

    public void BeginFlood()
    {
        ThreadPool.SetMaxThreads(_configuration.threadCount, _configuration.threadCount / 2);
        foreach (TargetInfo target in _configuration.targets)
        {
            for (int i = 0; i < target.concurrency; i++)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(
                        async connection =>
                        {
                            try
                            {
                                await (connection as LimitedHttpConnection).FloodHost();
                            }
                            catch (Exception ex)
                            {
                            }
                        },
                        new LimitedHttpConnection(target),
                        false);
                }
                catch (NotSupportedException ex)
                {
                    Console.Write(ex.Message);
                    return;
                }
            }
        }
    }
}