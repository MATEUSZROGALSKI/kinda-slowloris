using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

internal class ThrottledNetworkStream : NetworkStream
{
    private readonly TargetInfo _target;
    private readonly AutoResetEvent _resetEvent;
    private readonly SslStream? _sslStream;

    public ThrottledNetworkStream(Socket socket, TargetInfo target, AutoResetEvent resetEvent)
        : base(socket, true)
    {
        if (target.timeout < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        _target = target;
        _resetEvent = resetEvent;
        // if target is secure - HTTPS
        if (_target.isSecure)
        {
            // create ssl stream and authenticate as client
            _sslStream = new(
                this, 
                false, 
                new RemoteCertificateValidationCallback(ValidateServerCertificate!), 
                null);
            _sslStream.AuthenticateAsClient(new SslClientAuthenticationOptions
            {
                TargetHost = _target.host,
                AllowRenegotiation = true
            });
        }
    }

    // accept any kind of ssl certificate
    private bool ValidateServerCertificate(object sender, 
        X509Certificate certificate,
        X509Chain chain, 
        SslPolicyErrors sslPolicyErrors)
    {
        return true;
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
            //
            // if target is HTTPS ( secure ) then use sslStream
            // otherwise just write data without encryption
            if (_target.isSecure) _sslStream!.WriteByte(b);
            else base.WriteByte(b);
            // record that the byte was actually sent
            Statistics.RecordStats(_target.host, 1);
            // delay sending next bye by the amount specified
            // in target configuration
            await Task.Delay(_target.timeout);

            if (_resetEvent.WaitOne(0))
                break;
        }
    }
}
