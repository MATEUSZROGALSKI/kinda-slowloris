using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

internal interface IThrottledStream
{
    Task WriteAsync(byte[] buffer);
}

internal sealed class ThrottledStreamFactory
{
    public static IThrottledStream CreateStream(Connection connection, TargetInfo target, WaitHandle signalHandle) =>
        target.isSecure ?
            new SSLThrottledStream(connection, target, signalHandle) :
            new ThrottledStream(connection, target, signalHandle);
}

internal abstract class ThrottledStreamWriter
{
    protected readonly TargetInfo _target;
    protected readonly WaitHandle _signalHandle;
    protected readonly Connection _connection;

    private Stream? _stream;
    
    public ThrottledStreamWriter(Connection connection, TargetInfo target, WaitHandle signalHandle)
    {
        if (target.timeout < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        _target = target;
        _signalHandle = signalHandle;
        _connection = connection;
    }

    protected abstract Stream CreateStream(Connection connection);

    protected async Task InternalWriteAsync(byte[] buffer)
    {
        if (_stream is null)
            _stream = CreateStream(_connection!);

        int bytesSent = 0;
        try
        {
            foreach (byte b in buffer)
            {
                if (!_connection.Connected || !_stream.CanWrite)
                    break;

                _stream!.WriteByte(b);
                ++bytesSent;
                await Task.Delay(_target.timeout);

                if (_signalHandle.WaitOne(0))
                    break;
            }
        }
        finally
        {
            Statistics.Instance.RecordBytesSent(_target.host, bytesSent);
        }
    }
}

internal sealed class ThrottledStream : ThrottledStreamWriter, IThrottledStream
{
    public ThrottledStream(Connection connection, TargetInfo target, WaitHandle signalHandle)
        : base(connection, target, signalHandle) { }

    protected override Stream CreateStream(Connection connection) =>
        new NetworkStream(connection);

    public async Task WriteAsync(byte[] buffer) =>
        await InternalWriteAsync(buffer);
}

internal sealed class SSLThrottledStream : ThrottledStreamWriter, IThrottledStream
{
    public SSLThrottledStream(Connection connection, TargetInfo target, WaitHandle signalHandle)
        : base(connection, target, signalHandle) { }

    private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

    protected override Stream CreateStream(Connection connection)
    {
        var baseStream = new NetworkStream(connection);
        var stream = new SslStream(
            baseStream,
            false,
            new RemoteCertificateValidationCallback(ValidateServerCertificate!),
            null);
        stream.AuthenticateAsClient(new SslClientAuthenticationOptions
        {
            TargetHost = _target.host,
            AllowRenegotiation = true
        });
        return stream;
    }

    public async Task WriteAsync(byte[] buffer) =>
        await InternalWriteAsync(buffer);
}