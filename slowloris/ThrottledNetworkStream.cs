using System.Net.Sockets;

internal class ThrottledNetworkStream
{
    private readonly NetworkStream _stream;
    private readonly TargetInfo _target;

    public ThrottledNetworkStream(Socket socket, TargetInfo target)
    {
        if (target.timeout <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        _stream = new (socket, true);
        _target = target;
    }

    public async Task WriteAsync(byte[] buffer)
    {
        foreach(byte b in buffer)
        {
            _stream.WriteByte(b);
            Statistics.RecordStats(_target.host, 1);
            await Task.Delay(_target.timeout);
        }
    }

    public void CloseAndDispose()
    {
        _stream.Flush();
        _stream.Close(100);
        _stream.Dispose();
    }
}
