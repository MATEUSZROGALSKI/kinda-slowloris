
using System.Net.Security;

internal class SSLThrottledNetworkStream
{
    private readonly SslStream _stream;

    public SSLThrottledNetworkStream(ThrottledNetworkStream networkStream)
    {
        _stream = new SslStream(networkStream);
    }
}