using System.Net.Sockets;

internal class ThrottledNetworkStream
{
    private readonly NetworkStream _stream;
    private readonly TargetInfo _target;
    private readonly AutoResetEvent _resetEvent;

    public ThrottledNetworkStream(Socket socket, TargetInfo target, AutoResetEvent resetEvent)
    {
        if (target.timeout < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        _stream = new (socket, true);
        _target = target;
        _resetEvent = resetEvent;
    }

    // writes network message as slow as indicated
    // in the target configuration
    public async Task WriteAsync(byte[] buffer)
    {
        foreach(byte b in buffer)
        {
            // send the byte and acknowledge the byte was sent (TCP)
            // we can check if socket is writeable before
            // but we rely on this method internal exception mechanism
            // to just throw an exception in case socket was disconnected
            _stream.WriteByte(b);
            // record that the byte was actually sent
            Statistics.RecordStats(_target.host, 1);
            // delay sending next bye by the amount specified
            // in target configuration
            await Task.Delay(_target.timeout);

            if (_resetEvent.WaitOne(0))
                break;
        }
    }

    public void CloseAndDispose()
    {
        _stream.Flush();
        _stream.Close(100);
        _stream.Dispose();
    }
}
